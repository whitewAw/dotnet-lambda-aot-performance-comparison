resource "aws_dynamodb_table" "lambda-demo" {
  name         = "${var.environment}-lambda-demo-db"
  billing_mode = "PAY_PER_REQUEST"
  hash_key     = "Id"
  # deletion_protection_enabled = true

  attribute {
    name = "Id"
    type = "S"
  }

  ttl {
    attribute_name = "TTL"
    enabled        = true
  }

  tags = var.tags
}
