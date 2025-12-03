# Timeout for Lambda functions
variable "lambda_function_timeout" {
  description = "The amount of time your Lambda Function has to run in seconds."
  type        = number
  default     = 10
}
variable "lambda_function_architecture" {
  description = "Instruction set architecture for Lambda functions (e.g., arm64, x86_64)."
  type        = string
  default     = "x86_64"
  validation {
    condition     = contains(["x86_64", "arm64"], var.lambda_function_architecture)
    error_message = "lambda_function_architecture must be either 'x86_64' or 'arm64'"
  }
}

variable "aws_region" {
  description = "AWS region to deploy resources"
  type        = string
}

variable "dynamodb_ttl_days" {
  description = "Default TTL days for DynamoDB items."
  type        = number
  default     = 5
}

# Required variables for tagging/naming standard
variable "environment" {
  type        = string
  description = "Determines the environment of the resource"
  validation {
    condition     = contains(["dev", "test", "prod"], var.environment)
    error_message = "environment must be one of: dev, test, prod"
  }
}
variable "source_code" {
  type = string
  description = "URL to the source code repository"
  default     = "https://dev.azure.com/bokf/DSG.BackOffice.BAU/_git/AwsLambdaAotSample"
}

variable "log_group_retention_in_days" {
  description = "CloudWatch log group retention in days"
  type        = number
}



