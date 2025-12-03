resource "aws_lambda_function" "this" {
  function_name = var.function_name
  handler       = var.handler
  runtime       = var.runtime
  filename      = var.zip_file
  timeout       = var.timeout
  architectures = [var.architecture]
  environment {
    variables = var.environment_variables
  }
  tags = var.tags

  role = var.lambda_role_arn
}

resource "aws_cloudwatch_log_group" "lambda" {
  name              = "/aws/lambda/${aws_lambda_function.this.function_name}"
  retention_in_days = var.log_group_retention_in_days
  tags              = var.tags
  depends_on        = [aws_lambda_function.this]
}

