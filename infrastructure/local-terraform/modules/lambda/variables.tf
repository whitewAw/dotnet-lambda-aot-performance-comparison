variable "function_name" {
	description = "Name of the Lambda function for batch creation"
	type        = string
}

variable "handler" {
	description = "Handler for the Lambda function"
	type        = string
}

variable "zip_file" {
	description = "Path to the Lambda deployment package zip file"
	type        = string
}

variable "timeout" {
	description = "Timeout (in seconds) for the Lambda function"
	type        = number
}

variable "architecture" {
	description = "Lambda architecture (e.g., x86_64, arm64)"
	type        = string
}

variable "tags" {
	description = "Tags to apply to all resources in this module"
	type        = map(string)
}

variable "environment_variables" {
	description = "Environment variables for the Lambda function"
	type        = map(string)
}

variable "dynamodb_table_arn" {
	description = "ARN of the DynamoDB table used by the Lambda function"
	type        = string
}

variable "runtime" {
  description = "Lambda runtime (e.g., provided.al2023, dotnet8, etc.)"
  type        = string
}

variable "log_group_retention_in_days" {
	description = "CloudWatch log group retention in days"
	type        = number
}

variable "lambda_role_arn" {
  description = "ARN of the IAM role to use for the Lambda function. Must be created outside this module."
  type        = string
}