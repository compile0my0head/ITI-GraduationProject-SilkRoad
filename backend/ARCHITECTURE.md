# Clean Architecture Pattern - GetAllProducts Example

This document explains how the **GetAllProducts** use case works across all layers.

## 🎯 Architecture Overview

\\\
Request Flow:
1. HTTP Request → ProductController (Presentation)
2. Controller → MediatR → GetAllProductsHandler (Application)
3. Handler → IProductRepository → ProductRepository (Infrastructure)
4. Repository → Database (or Mock data)
5. Response flows back up the chain
\\\

## 📁 File Structure

\\\
Domain/
└── Entities/
    └── Product.cs              ← Domain entity with business logic

Application/
├── DTOs/Products/
│   └── ProductDto.cs           ← Data transfer objects
├── Common/Interfaces/
│   └── IProductRepository.cs   ← Repository contract
└── Features/Products/
    ├── Queries/
    │   └── GetAllProductsQuery.cs    ← Query request
    └── Handlers/
        └── GetAllProductsHandler.cs  ← Query handler (business logic)

Infrastructure/
└── Repositories/
    ├── ProductRepository.cs          ← Real DB implementation
    └── MockProductRepository.cs      ← Mock for testing

Presentation/
└── Controllers/
    └── ProductController.cs          ← API endpoints
\\\

## 🔄 Request Flow Example

### 1. Client Makes Request
\\\http
GET /api/product?storeId=1&inStockOnly=true
\\\

### 2. Controller Receives Request
\\\csharp
// ProductController.cs
[HttpGet]
public async Task<IActionResult> GetAllProducts(int? storeId, bool? inStockOnly)
{
    var query = new GetAllProductsQuery { StoreId = storeId, InStockOnly = inStockOnly };
    var response = await _mediator.Send(query); // MediatR routes to handler
    return Ok(response);
}
\\\

### 3. MediatR Routes to Handler
\\\csharp
// GetAllProductsHandler.cs
public async Task<GetProductsResponse> Handle(GetAllProductsQuery request, ...)
{
    // 1. Get data from repository
    var products = await _productRepository.GetAllAsync();
    
    // 2. Apply filters
    if (request.InStockOnly) products = products.Where(p => p.InStock);
    
    // 3. Map to DTOs
    var dtos = products.Select(p => new ProductDto { ... });
    
    // 4. Return response
    return new GetProductsResponse { Products = dtos };
}
\\\

### 4. Repository Fetches Data
\\\csharp
// MockProductRepository.cs (or ProductRepository.cs with DB)
public Task<List<Product>> GetAllAsync()
{
    return Task.FromResult(_fakeProducts); // Mock data
    // OR: return await _context.Products.ToListAsync(); // Real DB
}
\\\

### 5. Response Returns to Client
\\\json
{
  "products": [
    {
      "id": 1,
      "name": "Laptop",
      "price": 999.99,
      "inStock": true
    }
  ],
  "totalCount": 1,
  "message": "Successfully retrieved 1 product(s)"
}
\\\

## 🎨 Design Patterns Used

### 1. **CQRS (Command Query Responsibility Segregation)**
- Queries (read) separated from Commands (write)
- \GetAllProductsQuery\ is a query - it doesn't modify data

### 2. **Mediator Pattern**
- MediatR decouples controllers from handlers
- Controller doesn't know which handler executes the query

### 3. **Repository Pattern**
- \IProductRepository\ abstracts data access
- Easy to switch between Mock and Real implementations

### 4. **Dependency Inversion**
- Application layer defines \IProductRepository\ interface
- Infrastructure layer implements it
- High-level modules don't depend on low-level modules

## 🚀 How to Add a New Use Case

Follow this pattern for any new feature:

### Example: Create "GetProductById" 

1. **Create Query** (\Application/Features/Products/Queries/\):
\\\csharp
public record GetProductByIdQuery(int Id) : IRequest<ProductDto>;
\\\

2. **Create Handler** (\Application/Features/Products/Handlers/\):
\\\csharp
public class GetProductByIdHandler : IRequestHandler<GetProductByIdQuery, ProductDto>
{
    private readonly IProductRepository _repository;
    
    public async Task<ProductDto> Handle(GetProductByIdQuery request, ...)
    {
        var product = await _repository.GetByIdAsync(request.Id);
        return new ProductDto { ... };
    }
}
\\\

3. **Add Controller Endpoint**:
\\\csharp
[HttpGet("{id}")]
public async Task<IActionResult> GetById(int id)
{
    var result = await _mediator.Send(new GetProductByIdQuery(id));
    return Ok(result);
}
\\\

4. **Test** - That's it! MediatR wires everything automatically.

## 📚 Key Benefits

✅ **Testable** - Mock repository for unit tests  
✅ **Maintainable** - Each layer has clear responsibility  
✅ **Scalable** - Easy to add new features following the same pattern  
✅ **Flexible** - Swap implementations without changing business logic  

## 🔧 Switching from Mock to Real Database

In \Program.cs\, change:
\\\csharp
// From:
builder.Services.AddScoped<IProductRepository, MockProductRepository>();

// To:
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
\\\

## 📖 Further Reading

- [Clean Architecture by Uncle Bob](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [MediatR Documentation](https://github.com/jbogard/MediatR)
- [Repository Pattern](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-design)
