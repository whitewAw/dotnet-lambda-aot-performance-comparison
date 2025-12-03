terraform {
  backend "local" {}
}

provider "aws" {
  region  = var.aws_region
}

# Shared IAM Role for Lambdas
resource "aws_iam_role" "lambda_role" {
  name = "${var.environment}-lambda-aot-demo-role"
  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [{
      Action = "sts:AssumeRole"
      Effect = "Allow"
      Principal = {
        Service = "lambda.amazonaws.com"
      }
    }]
  })
  tags = local.tags
}

resource "aws_iam_role_policy" "lambda_policy" {
  name   = "${var.environment}-lambda-aot-demo-policy"
  role   = aws_iam_role.lambda_role.id
  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Effect = "Allow"
        Action = [
          "dynamodb:DescribeTable",
          "dynamodb:GetItem",
          "dynamodb:UpdateItem",
        ]
        Resource = module.dynamodb.table_arn
      },
      {
        Effect = "Allow"
        Action = [
          "logs:CreateLogGroup",
          "logs:CreateLogStream",
          "logs:PutLogEvents"
        ]
        Resource = "arn:aws:logs:*:*:*"
      }
      ,
      {
        Effect = "Allow"
        Action = [
          "lambda:InvokeFunction"
        ]
        Resource = "arn:aws:lambda:*:function:*"
      },
      {
        Effect = "Allow"
        Action = [
          "lambda:GetFunction"
        ]
        Resource = "arn:aws:lambda:*:function:*"
      }
    ]
  })
}

# DynamoDB Table (module)
module "dynamodb" {
  source      = "./modules/dynamodb"
  environment = var.environment
  tags        = local.tags
}

# Lambdas 
module "aot8_lambda" {
  source                = "./modules/lambda"
  function_name         = "${var.environment}-lambda-aot8-al2023-demo"
  handler               = "bootstrap"
  runtime               = "provided.al2023"
  zip_file              = "${path.root}/../artifacts/LambdaAOTDemo8-lambda.zip"
  timeout               = var.lambda_function_timeout
  architecture          = var.lambda_function_architecture
  tags                  = local.tags
  environment_variables = {
    DYNAMODB_TABLE_NAME = module.dynamodb.table_name
    DYNAMODB_TTL_DAYS   = var.dynamodb_ttl_days
  }
  dynamodb_table_arn    = module.dynamodb.table_arn
  log_group_retention_in_days = var.log_group_retention_in_days
  lambda_role_arn             = aws_iam_role.lambda_role.arn
}

module "aot_lambda_dotnet8" {
  source                = "./modules/lambda"
  function_name         = "${var.environment}-lambda-aot8-dotnet8-demo"
  handler               = "bootstrap"
  runtime               = "dotnet8"
  zip_file              = "${path.root}/../artifacts/LambdaAOTDemo8-lambda.zip"
  timeout               = var.lambda_function_timeout
  architecture          = var.lambda_function_architecture
  tags                  = local.tags
  environment_variables = {
    DYNAMODB_TABLE_NAME = module.dynamodb.table_name
    DYNAMODB_TTL_DAYS   = var.dynamodb_ttl_days
  }
  dynamodb_table_arn    = module.dynamodb.table_arn
  log_group_retention_in_days = var.log_group_retention_in_days
  lambda_role_arn             = aws_iam_role.lambda_role.arn
}

module "aot9_lambda" {
  source                = "./modules/lambda"
  function_name         = "${var.environment}-lambda-aot9-al2023-demo"
  handler               = "bootstrap"
  runtime               = "provided.al2023"
  zip_file              = "${path.root}/../artifacts/LambdaAOTDemo9-lambda.zip"
  timeout               = var.lambda_function_timeout
  architecture          = var.lambda_function_architecture
  tags                  = local.tags
  environment_variables = {
    DYNAMODB_TABLE_NAME = module.dynamodb.table_name
    DYNAMODB_TTL_DAYS   = var.dynamodb_ttl_days
  }
  dynamodb_table_arn    = module.dynamodb.table_arn
  log_group_retention_in_days = var.log_group_retention_in_days
  lambda_role_arn             = aws_iam_role.lambda_role.arn
}

module "aot9_lambda_dotnet8" {
  source                = "./modules/lambda"
  function_name         = "${var.environment}-lambda-aot9-dotnet8-demo"
  handler               = "bootstrap"
  runtime               ="dotnet8"
  zip_file              = "${path.root}/../artifacts/LambdaAOTDemo9-lambda.zip"
  timeout               = var.lambda_function_timeout
  architecture          = var.lambda_function_architecture
  tags                  = local.tags
  environment_variables = {
    DYNAMODB_TABLE_NAME = module.dynamodb.table_name
    DYNAMODB_TTL_DAYS   = var.dynamodb_ttl_days
  }
  dynamodb_table_arn    = module.dynamodb.table_arn
  log_group_retention_in_days = var.log_group_retention_in_days
  lambda_role_arn             = aws_iam_role.lambda_role.arn
}

module "aot10_lambda" {
  source                = "./modules/lambda"
  function_name         = "${var.environment}-lambda-aot10-al2023-demo"
  handler               = "bootstrap"
  runtime               = "provided.al2023"
  zip_file              = "${path.root}/../artifacts/LambdaAOTDemo10-lambda.zip"
  timeout               = var.lambda_function_timeout
  architecture          = var.lambda_function_architecture
  tags                  = local.tags
  environment_variables = {
    DYNAMODB_TABLE_NAME = module.dynamodb.table_name
    DYNAMODB_TTL_DAYS   = var.dynamodb_ttl_days
  }
  dynamodb_table_arn    = module.dynamodb.table_arn
  log_group_retention_in_days = var.log_group_retention_in_days
  lambda_role_arn             = aws_iam_role.lambda_role.arn
}

module "aot10_lambda_dotnet8" {
  source                = "./modules/lambda"
  function_name         = "${var.environment}-lambda-aot10-dotnet8-demo"
  handler               = "bootstrap"
  runtime               = "dotnet8"
  zip_file              = "${path.root}/../artifacts/LambdaAOTDemo10-lambda.zip"
  timeout               = var.lambda_function_timeout
  architecture          = var.lambda_function_architecture
  tags                  = local.tags
  environment_variables = {
    DYNAMODB_TABLE_NAME = module.dynamodb.table_name
    DYNAMODB_TTL_DAYS   = var.dynamodb_ttl_days
  }
  dynamodb_table_arn    = module.dynamodb.table_arn
  log_group_retention_in_days = var.log_group_retention_in_days
  lambda_role_arn             = aws_iam_role.lambda_role.arn
}

module "r2r_lambda" {
  source                = "./modules/lambda"
  function_name         = "${var.environment}-lambda-r2r-dotnet8-demo"
  handler               = "LambdaReadyToRunDemo::LambdaReadyToRunDemo.Function_FunctionHandler_Generated::FunctionHandler"
  runtime               = "dotnet8"
  zip_file              = "${path.root}/../artifacts/LambdaR2RDemo-lambda.zip"
  timeout               = var.lambda_function_timeout
  architecture          = var.lambda_function_architecture
  tags                  = local.tags
  environment_variables = {
    DYNAMODB_TABLE_NAME = module.dynamodb.table_name
    DYNAMODB_TTL_DAYS   = var.dynamodb_ttl_days
  }
  dynamodb_table_arn    = module.dynamodb.table_arn
  log_group_retention_in_days = var.log_group_retention_in_days
  lambda_role_arn             = aws_iam_role.lambda_role.arn
}

module "regular_lambda" {
  source                = "./modules/lambda"
  function_name         = "${var.environment}-lambda-regular-dotnet8-demo"
  handler               = "LambdaRegularDemo::LambdaRegularDemo.Function_FunctionHandler_Generated::FunctionHandler"
  runtime               = "dotnet8"
  zip_file              = "${path.root}/../artifacts/LambdaRegularDemo-lambda.zip"
  timeout               = var.lambda_function_timeout
  architecture          = var.lambda_function_architecture
  tags                  = local.tags
  environment_variables = {
    DYNAMODB_TABLE_NAME = module.dynamodb.table_name
    DYNAMODB_TTL_DAYS   = var.dynamodb_ttl_days
  }
  dynamodb_table_arn    = module.dynamodb.table_arn
  log_group_retention_in_days = var.log_group_retention_in_days
  lambda_role_arn             = aws_iam_role.lambda_role.arn
}


module "lambda_invoker" {
  source                = "./modules/lambda"
  function_name         = "${var.environment}-lambda-invoker-demo"
  handler               = "bootstrap"
  runtime               = "provided.al2023"
  zip_file              = "${path.root}/../artifacts/LambdaInvoker-lambda.zip"
  timeout               = 120
  architecture          = var.lambda_function_architecture
  tags                  = local.tags
  environment_variables = {}
  dynamodb_table_arn    = module.dynamodb.table_arn
  log_group_retention_in_days = var.log_group_retention_in_days
  lambda_role_arn             = aws_iam_role.lambda_role.arn
}
