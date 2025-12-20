# structureV1

Angular project structure for an MVP e-commerce and marketing automation platform.

## Project Structure

```
src/
└── app/
    ├── core/                    # Global low-level functionality
    │   ├── auth/
    │   ├── services/
    │   └── interceptors/
    │
    ├── shared/                  # Reusable UI components and utilities
    │   ├── components/
    │   ├── directives/
    │   ├── pipes/
    │   └── utils/
    │
    └── features/                # Business features
        ├── auth/
        ├── stores/
        ├── products/
        ├── campaigns/
        ├── campaign-posts/
        ├── customers/
        ├── orders/
        ├── platforms/
        ├── teams/
        ├── automation-tasks/
        └── chatbot-faq/
```

## Features

### Core Modules
- **Authentication**: Login, registration, and auth guards
- **HTTP Services**: Centralized HTTP handling
- **Interceptors**: Request/response interceptors

### Business Features
- **Stores**: Store management and dashboard
- **Products**: Product CRUD operations
- **Campaigns**: Marketing campaign management
- **Campaign Posts**: Social media post scheduling
- **Customers**: Customer management
- **Orders**: Order processing and tracking
- **Platforms**: Social media platform integrations
- **Teams**: Team collaboration and roles
- **Automation Tasks**: Task scheduling and automation
- **Chatbot FAQ**: FAQ management for chatbot

## Getting Started

### Prerequisites
- Node.js (v18 or higher)
- npm or yarn
- Angular CLI

### Installation

```bash
npm install
```

### Development Server

```bash
ng serve
```

Navigate to `http://localhost:4200/`

### Build

```bash
ng build
```

## Database Schema

The application is built on the following database schema:

- **User**: User authentication and profile
- **Store**: Multi-store support
- **Team & TeamMember**: Collaborative team management
- **Product**: Product catalog
- **Campaign**: Marketing campaigns with stages
- **CampaignPost**: Scheduled social media posts
- **Customer**: Customer management
- **Order & OrderProduct**: Order processing
- **SocialPlatform**: Social media integrations
- **AutomationTask**: Scheduled automation tasks
- **ChatbotFAQ**: Chatbot knowledge base

## Architecture

This project follows a feature-based architecture with clear separation of concerns:

- **Core**: Never imports UI, contains low-level services
- **Shared**: Reusable components, no feature-specific logic
- **Features**: Business logic organized by domain

## License

MIT
