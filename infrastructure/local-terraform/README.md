# Lambda .NET Compilation Modes Demo - Terraform Infrastructure

This Terraform infrastructure deploys AWS Lambda functions demonstrating different .NET compilation modes across multiple .NET versions (8, 9, 10) to compare Native AOT, Ready-to-Run, and Regular JIT performance.

## Overview

This Terraform configuration deploys:

### Lambda Functions
- **AOT Lambda Functions** (Native AOT compilation):
  - `.NET 8 AOT` on `provided.al2023` runtime
  - `.NET 8 AOT` on `dotnet8` runtime (for comparison)
  - `.NET 9 AOT` on `provided.al2023` runtime
  - `.NET 9 AOT` on `dotnet8` runtime (for comparison)
  - `.NET 10 AOT` on `provided.al2023` runtime
  - `.NET 10 AOT` on `dotnet8` runtime (for comparison)
- **Ready-to-Run Lambda**: .NET 8 with R2R optimization (runtime: `dotnet8`)
- **Regular Lambda**: Standard .NET 8 with JIT compilation (runtime: `dotnet8`)
- **Lambda Invoker**: Performance testing orchestrator (runtime: `provided.al2023`)

### Supporting Infrastructure
- **DynamoDB Table**: For storing batch data with TTL configuration
- **IAM Role**: Shared role with permissions for DynamoDB, CloudWatch Logs, and Lambda invocation
- **CloudWatch Log Groups**: Automatic log groups for each Lambda function with configurable retention

## Prerequisites

- [Terraform](https://www.terraform.io/downloads.html) installed (v1.0+)
- AWS CLI configured with appropriate credentials
- PowerShell (for building Lambda artifacts)
- Docker (required for building Lambda deployment packages)

## Building Lambda Artifacts

Before deploying the infrastructure, you need to build the Lambda deployment packages using Docker:

```powershell
# Navigate to the infrastructure directory
cd infrastructure

# Run the build script
.\build-lambda-docker-zip.ps1
```

This script will:
1. Build a Docker image containing all Lambda functions
2. Extract compiled artifacts from the container
3. Create deployment ZIP files in the `artifacts/` directory:
   - `LambdaAOTDemo8-lambda.zip` (.NET 8 Native AOT)
   - `LambdaAOTDemo9-lambda.zip` (.NET 9 Native AOT)
   - `LambdaAOTDemo10-lambda.zip` (.NET 10 Native AOT)
   - `LambdaR2RDemo-lambda.zip` (Ready-to-Run)
   - `LambdaRegularDemo-lambda.zip` (Regular .NET)
   - `LambdaInvoker-lambda.zip` (Testing orchestrator)

**Note**: Docker must be running before executing the build script.

## Deployment Commands

### Initialize Terraform

Initialize the Terraform working directory and download required providers:

```powershell
terraform init
```

### Plan Infrastructure Changes

Preview the infrastructure changes that will be made:

```powershell
terraform plan -var-file="dev.tfvars"
```

### Apply Infrastructure

Deploy the infrastructure to AWS:

```powershell
terraform apply -var-file="dev.tfvars"
```

### Destroy Infrastructure

Remove all resources created by Terraform:

```powershell
terraform destroy -var-file="dev.tfvars"
```

### View Outputs

After deployment, view the created resources:

```powershell
# Display all outputs (function ARNs, table name, etc.)
terraform output

# Get specific output value
terraform output dynamodb_table_name
```

## Testing the Deployment

### Invoke a Lambda Function

Test individual Lambda functions after deployment:

```powershell
# Test .NET 9 AOT function
aws lambda invoke `
  --function-name dev-lambda-aot9-al2023-demo `
  --payload '{"test":"data"}' `
  response.json

# View the response
Get-Content response.json | ConvertFrom-Json
```

### Monitor Performance

View CloudWatch logs and metrics:

```powershell
# Tail logs for a specific function
aws logs tail /aws/lambda/dev-lambda-aot9-al2023-demo --follow

# Get recent log events
aws logs tail /aws/lambda/dev-lambda-aot9-al2023-demo --since 1h

# Compare cold start metrics
aws cloudwatch get-metric-statistics `
  --namespace AWS/Lambda `
  --metric-name Duration `
  --dimensions Name=FunctionName,Value=dev-lambda-aot9-al2023-demo `
  --start-time (Get-Date).AddHours(-24) `
  --end-time (Get-Date) `
  --period 3600 `
  --statistics Average,Minimum,Maximum
```

### Use Lambda Invoker for Performance Testing

The Lambda Invoker function can orchestrate performance tests across all deployed functions:

```powershell
# Invoke the orchestrator (configure with target function ARNs)
aws lambda invoke `
  --function-name dev-lambda-invoker-demo `
  --payload '{"iterations":10}' `
  performance-results.json
```

## Configuration

Configuration variables are defined in `dev.tfvars`. Key variables include:

- `aws_region`: AWS region for deployment
- `environment`: Environment name (e.g., "dev", "prod")
- `lambda_function_timeout`: Lambda execution timeout in seconds
- `lambda_function_architecture`: Lambda architecture (x86_64 or arm64)
- `dynamodb_ttl_days`: DynamoDB TTL in days
- `log_group_retention_in_days`: CloudWatch log retention period

## Module Structure

```
.
├── main.tf                    # Main Terraform configuration
├── variables.tf               # Input variables
├── locals.tf                  # Local values
├── outputs.tf                 # Output values
├── versions.tf                # Terraform and provider versions
├── dev.tfvars                 # Environment-specific variables
├── modules/
│   ├── dynamodb/              # DynamoDB table module
│   │   ├── main.tf
│   │   ├── variables.tf
│   │   └── outputs.tf
│   └── lambda/                # Lambda function module
│       ├── main.tf
│       ├── variables.tf
│       └── outputs.tf
└── README.md                  # This file
```

## Notes

- The Lambda functions share a common IAM role for consistency and simplified management
- All resources are tagged with environment and project information
- The DynamoDB table uses PAY_PER_REQUEST billing mode
- CloudWatch log groups are created automatically for each Lambda function

## Troubleshooting

### IAM Role Already Exists

If you encounter an error about an existing IAM role, you can import it:

```powershell
terraform import aws_iam_role.lambda_role <environment>-lambda-aot-demo-role
```

Replace `<environment>` with your environment name (e.g., "dev").

### Lambda Function Update Not Detected

If you rebuild artifacts but Terraform doesn't detect changes:

```powershell
# Force replacement of specific Lambda function
terraform apply -var-file="dev.tfvars" -replace="module.aot9_lambda.aws_lambda_function.this"

# Or taint the resource to force recreation
terraform taint module.aot9_lambda.aws_lambda_function.this
terraform apply -var-file="dev.tfvars"
```

### Docker Build Issues

If the build script fails:

```powershell
# Ensure Docker is running
docker ps

# Clean up any leftover containers/images
docker rm -f temp-nativeaot
docker rmi gl_posting_interface

# Run the build script again
.\build-lambda-docker-zip.ps1
```

### DynamoDB Access Issues

Verify the Lambda functions have proper permissions:

```powershell
# Check IAM role policies
aws iam get-role-policy `
  --role-name dev-lambda-aot-demo-role `
  --policy-name dev-lambda-aot-demo-policy
```

## Performance Comparison Tips

- **Cold Start Testing**: Delete and recreate functions to measure cold starts consistently
- **Memory Configuration**: Adjust `lambda_function_memory` in `dev.tfvars` to see impact on performance
- **Architecture**: Compare x86_64 vs arm64 by changing `lambda_function_architecture`
- **Runtime Comparison**: Notice how the same AOT binary runs on both `provided.al2023` and `dotnet8` runtimes
