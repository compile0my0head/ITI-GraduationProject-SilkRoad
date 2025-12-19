using Application.Common.Interfaces;
using Application.DTOs.Orders;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Application.Services;

/// <summary>
/// Service for processing orders from chatbot (n8n/Facebook Messenger)
/// Handles customer creation/lookup, product matching, and order creation
/// </summary>
public class ChatbotOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<ChatbotOrderService> _logger;

    public ChatbotOrderService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<ChatbotOrderService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Process chatbot order request
    /// </summary>
    public async Task<OrderDto> ProcessChatbotOrderAsync(
        ChatbotOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        // Step 1: Get StoreId from Facebook Page ID
        var platform = await _unitOfWork.SocialPlatforms.GetByExternalPageIdAsync(
            request.PageId, 
            cancellationToken);

        if (platform == null)
        {
            throw new InvalidOperationException(
                $"Facebook page with ID '{request.PageId}' is not connected to any store. " +
                "Please connect this page in the social platforms settings.");
        }

        var storeId = platform.StoreId;
        _logger.LogInformation(
            "Processing chatbot order for store {StoreId} from page {PageId}",
            storeId,
            request.PageId);

        // Step 2: Find or create customer
        var customer = await FindOrCreateCustomerAsync(request.Customer, storeId, cancellationToken);

        // Step 3: Match products and calculate total
        var (orderProducts, totalPrice) = await MatchProductsAsync(
            request.Items,
            storeId,
            cancellationToken);

        // Step 4: Create order
        var order = new Order
        {
            StoreId = storeId,
            CustomerId = customer.Id,
            TotalPrice = totalPrice,
            Status = OrderStatus.Pending, // Always start as Pending
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Orders.AddAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Step 5: Create order products
        foreach (var (product, quantity, unitPrice) in orderProducts)
        {
            var orderProduct = new OrderProduct
            {
                OrderId = order.Id,
                ProductId = product.Id,
                Quantity = quantity,
                UnitPrice = unitPrice
            };

            await _unitOfWork.OrderProducts.AddAsync(orderProduct, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Successfully created chatbot order {OrderId} with {ItemCount} items for customer {CustomerId}",
            order.Id,
            orderProducts.Count,
            customer.Id);

        // Reload order with navigation properties
        var savedOrder = await _unitOfWork.Orders.GetByIdAsync(order.Id, cancellationToken);
        return _mapper.Map<OrderDto>(savedOrder);
    }

    /// <summary>
    /// Find existing customer or create new one
    /// Search priority: PSID > Phone > Create New
    /// </summary>
    private async Task<Customer> FindOrCreateCustomerAsync(
        ChatbotCustomerInfo customerInfo,
        Guid storeId,
        CancellationToken cancellationToken)
    {
        // Try to find by PSID first
        var customer = await _unitOfWork.Customers.GetByPSIDAsync(
            customerInfo.Psid,
            storeId,
            cancellationToken);

        if (customer != null)
        {
            _logger.LogInformation(
                "Found existing customer by PSID: {CustomerId}",
                customer.Id);
            return customer;
        }

        // Try to find by phone
        if (!string.IsNullOrWhiteSpace(customerInfo.Phone))
        {
            customer = await _unitOfWork.Customers.GetByPhoneAsync(
                customerInfo.Phone,
                storeId,
                cancellationToken);

            if (customer != null)
            {
                _logger.LogInformation(
                    "Found existing customer by phone: {CustomerId}",
                    customer.Id);
                
                // Update PSID if not set
                if (string.IsNullOrWhiteSpace(customer.PSID))
                {
                    customer.PSID = customerInfo.Psid;
                    await _unitOfWork.Customers.UpdateAsync(customer, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }
                
                return customer;
            }
        }

        // Create new customer
        var customerName = string.IsNullOrWhiteSpace(customerInfo.Name) 
            ? "Anonymous" 
            : customerInfo.Name;

        customer = new Customer
        {
            StoreId = storeId,
            CustomerName = customerName,
            Phone = customerInfo.Phone,
            BillingAddress = customerInfo.Address,
            PSID = customerInfo.Psid,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Customers.AddAsync(customer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Created new customer {CustomerId} for PSID {PSID}",
            customer.Id,
            customerInfo.Psid);

        return customer;
    }

    /// <summary>
    /// Match products by name and calculate total price
    /// Returns: (product, quantity, unitPrice) tuples and total price
    /// </summary>
    private async Task<(List<(Product product, int quantity, decimal unitPrice)> orderProducts, decimal totalPrice)> 
        MatchProductsAsync(
            List<ChatbotOrderItem> items,
            Guid storeId,
            CancellationToken cancellationToken)
    {
        var orderProducts = new List<(Product product, int quantity, decimal unitPrice)>();
        decimal totalPrice = 0;

        foreach (var item in items)
        {
            var product = await _unitOfWork.Products.GetByNameAsync(
                item.ProductName,
                storeId,
                cancellationToken);

            if (product == null)
            {
                _logger.LogWarning(
                    "Product not found for name '{ProductName}' in store {StoreId}. Skipping item.",
                    item.ProductName,
                    storeId);
                continue;
            }

            var unitPrice = product.ProductPrice;
            var lineTotal = unitPrice * item.Quantity;

            orderProducts.Add((product, item.Quantity, unitPrice));
            totalPrice += lineTotal;

            _logger.LogInformation(
                "Matched product {ProductId} ({ProductName}) - Qty: {Quantity}, Unit: {UnitPrice}, Total: {LineTotal}",
                product.Id,
                product.ProductName,
                item.Quantity,
                unitPrice,
                lineTotal);
        }

        if (orderProducts.Count == 0)
        {
            _logger.LogWarning(
                "No products matched for chatbot order. Creating order with 0 total.");
        }

        return (orderProducts, totalPrice);
    }
}
