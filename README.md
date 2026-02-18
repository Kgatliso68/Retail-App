# Azure Application
## Overview
ABCRetailAzureApp is an ASP.NET Core MVC web application that provides a comprehensive retail management platform integrated with Microsoft Azure cloud services. This application was developed to modernize ABC Retail's aging on-premises infrastructure by migrating to a scalable, cloud-based solution.
## Business Context
ABC Retail, a rapidly growing online retailer, previously managed its order processing system using legacy on-premises infrastructure with traditional relational databases and network shared drives. This legacy system struggled with:

High transaction volumes during peak shopping seasons
Storage inefficiencies and slow access times for product images
Unreliable message queuing through legacy middleware
Delayed message delivery and processing errors

This application addresses these challenges by leveraging Azure's cloud services to provide scalability, reliability, and improved performance.
### Key Features
#### 1. Customer Management

Create, view, and manage customer profiles
Store customer data in Azure Table Storage
Track customer purchase history and preferences

#### 2. Product Management

Complete CRUD operations for product catalog
Store product images in Azure Blob Storage
Manage product inventory and pricing
Support for multimedia content hosting

#### 3. Order Processing

Create and track customer orders
Process orders through Azure Queue Storage
Manage order status and fulfillment
Handle inventory updates via queued events

#### 4. Cloud Integration

Azure Table Storage: Scalable NoSQL storage for customer profiles and product data
Azure Blob Storage: Efficient media hosting for product images and multimedia files
Azure Queue Storage: Reliable message queuing for order processing and inventory management
Azure File Storage: Persistent logging and file storage for auditing and diagnostics

## Architecture
The application follows a layered architecture pattern:
#### 1.  Presentation Layer              
    (Controllers & Razor Views)          

#### 2.         Business Logic Layer            
    (Service Interfaces & Models)        

#### 3.     Data Access Layer               
    (Azure Service Implementations)      

#### 4.         Azure Cloud Services            
  (Tables, Blobs, Queues, Files)        

## Prerequisites

.NET 6.0 SDK or higher
Visual Studio 2022 or Visual Studio Code
Azure subscription with the following resources:

Azure Storage Account
Azure Table Storage
Azure Blob Storage
Azure Queue Storage
Azure File Storage


Azure Storage Explorer (optional, for debugging)

## Configuration
#### 1. Azure Storage Setup
Create an Azure Storage Account and note the connection string. Update appsettings.json:


#### 2. Service Registration

### Installation & Running Locally
**Step 1:** Clone the Repository
bashgit clone <repository-url>
cd ABCRetailAzureApp
**Step 2:** Restore Dependencies
bashdotnet restore
**Step 3:** Update Configuration
Edit appsettings.json with your Azure Storage connection string.
**Step 4:** Build the Application
bashdotnet build
**Step 5:** Run the Application
bashdotnet run
The application will be available at https://localhost:5001 or http://localhost:5000.
### Deployment to Azure App Service
#### Using Visual Studio

Right-click the project in Solution Explorer
Select Publish
Choose Azure as the target
Select Azure App Service (Windows) or Azure App Service (Linux)
Create a new App Service or select an existing one
Click Publish

#### Using Azure CLI
bash# Login to Azure
az login

 - Create a resource group
az group create --name ABCRetailRG --location eastus

- Create an App Service plan
az appservice plan create --name ABCRetailPlan --resource-group ABCRetailRG --sku B1

- Create a web app
az webapp create --name abc-retail-app --resource-group ABCRetailRG --plan ABCRetailPlan

 - Deploy the application
dotnet publish -c Release
cd bin/Release/net6.0/publish
zip -r ../deploy.zip .
az webapp deployment source config-zip --resource-group ABCRetailRG --name abc-retail-app --src ../deploy.zip
#### Post-Deployment Configuration
After deployment, configure application settings in the Azure Portal:

Navigate to your App Service
Go to Configuration > Application settings
Add the following settings:

AzureStorage:ConnectionString
AzureStorage:TableStorageEndpoint
AzureStorage:BlobStorageEndpoint
AzureStorage:QueueStorageEndpoint
AzureStorage:FileStorageEndpoint



## Usage
#### Managing Customers

1. Navigate to Customers from the main menu
2. Click Create New to add a customer
3. Fill in customer details (Name, Email, Phone, Address)
4. Click Save to store in Azure Table Storage

#### Managing Products

1. Navigate to Products from the main menu
2. Click Create New to add a product
3. Enter product details (Name, Description, Price, Category)
4. Upload product image (stored in Azure Blob Storage)
5. Click Save

#### Processing Orders

1. Navigate to Orders from the main menu
2. Click Create New Order
3. Select a customer from the dropdown
4. Add products to the order
5. Review order summary
6. Click Submit Order

  - Order details are saved to Azure Table Storage
  - Order processing message is queued in Azure Queue Storage
  - Inventory update message is queued for processing



## Azure Services Integration
**Azure Table Storage**

Purpose: Store structured NoSQL data for customers and products
Tables:

Customers - Customer profiles
Products - Product catalog
Orders - Order records



**Azure Blob Storage**

Purpose: Store product images and multimedia content
Containers:

product-images - Product photos
customer-uploads - Customer-uploaded content



**Azure Queue Storage**

Purpose: Asynchronous message processing for orders and inventory
Queues:

order-processing - Order fulfillment messages
inventory-updates - Stock level updates



**Azure File Storage**

Purpose: Application logging and file persistence
File Shares:

application-logs - System and application logs
reports - Generated reports and documents



## Future Enhancements
**Azure Event Hubs Integration**

Real-time inventory updates
Live order tracking
Personalized customer experiences
Real-time analytics and monitoring

**Azure Service Bus Integration**

Guaranteed message delivery
Complex message routing
Dead letter queue handling
Transaction support for critical operations

## Testing
**Local Testing**
bashdotnet test

**Manual Testing Checklist**

  - Create a new customer
  - Upload a product with image
  - Create an order
  - Verify data in Azure Storage Explorer
  - Check queue messages are being processed
  - Review application logs in Azure File Storage

## Troubleshooting
**Common Issues**
Issue: Connection to Azure Storage fails

Solution: Verify connection string in appsettings.json
Check firewall rules on Azure Storage Account

Issue: Images not displaying

Solution: Ensure blob container has public read access
Verify blob URL is correctly formatted

Issue: Orders not processing

Solution: Check Azure Queue Storage for messages
Review application logs in Azure File Storage

Issue: Application crashes on startup

Solution: Check all Azure services are properly configured
Verify all NuGet packages are restored

## Support & Documentation

Azure Storage Documentation
ASP.NET Core MVC Documentation
Azure App Service Documentation

## License
This project is developed as part of an academic assignment for cloud computing integration studies.
Contributors

Kgatliso - Project Developer
ABC Retail - Business Requirements

### Version History

v1.0 - Initial release with core Azure services integration

Customer Management
Product Management
Order Processing
Azure Table, Blob, Queue, and File Storage integration
