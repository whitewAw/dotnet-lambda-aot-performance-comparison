# .NET NativeAOT: Performance Revolution
## A Deep Dive into AOT vs ReadyToRun vs Regular .NET

[![Stand With Ukraine](https://img.shields.io/badge/Stand_With-Ukraine-FFD500?labelColor=005BBB)](https://github.com/vshymanskyy/StandWithUkraine/blob/main/docs/README.md)
[![Developed by](https://img.shields.io/badge/Developed_by-Alex_%28Oleksandr%29_Shevchenko-0066CC?logo=github)](https://github.com/whitewAw)

---

## 📋 Table of Contents

1. [Introduction & Objectives](#introduction--objectives)
2. [Understanding the Execution Models](#understanding-the-execution-models)
3. [Why NativeAOT Matters](#why-nativeaot-matters)
4. [Performance Results: The Numbers Don't Lie](#performance-results-the-numbers-dont-lie)
5. [Build & Packaging Deep Dive](#build--packaging-deep-dive)
6. [Coding for AOT: Patterns & Best Practices](#coding-for-aot-patterns--best-practices)
7. [Real-World Examples from This Repo](#real-world-examples-from-this-repo)
8. [Deployment Strategies](#deployment-strategies)
9. [Migration Guide](#migration-guide)
10. [When to Choose Which Approach](#when-to-choose-which-approach)
11. [FAQ & Troubleshooting](#faq--troubleshooting)
12. [Key Takeaways](#key-takeaways)
13. [Additional Resources](#additional-resources)
14. [Getting Started](#getting-started)
15. [Contributing](#contributing)
16. [License](#license)
17. [Acknowledgments](#acknowledgments)

---

## 🎯 Introduction & Objectives

### Who This Presentation Is For
.NET engineers familiar with Lambda, microservices, or performance-critical applications looking to understand modern compilation strategies.

### What You'll Learn
- **NativeAOT fundamentals** and how it compares to ReadyToRun and Regular .NET
- **Measured performance improvements** from real-world Lambda functions
- **Practical build and deployment** strategies
- **Code patterns** required for successful AOT adoption
- **Migration strategies** for existing applications

### Repository Context
This presentation uses a multi-mode Lambda demo repository with:
- 3 AOT Lambda functions (.NET 8, 9, 10)
- 1 ReadyToRun Lambda function
- 1 Regular .NET Lambda function
- Shared services and DynamoDB integration
- Containerized build pipeline

---

## 🔍 Understanding the Execution Models

### Regular .NET: Traditional JIT Approach

**How It Works:**
- Application ships as **Intermediate Language (IL)** bytecode
- **Just-In-Time (JIT)** compiler translates IL to native code at runtime
- Compilation happens on first method invocation
- Tiered compilation optimizes hot paths over time

**Characteristics:**
```
+------------------------------------------+
|  Application Start                       |
+------------------------------------------+
|  1. Load IL assemblies                   |
|  2. Initialize runtime (CLR)             |
|  3. JIT compile on first call            |
|  4. Execute native code                  |
|  5. Tier 0 -> Tier 1 optimization        |
+------------------------------------------+
```

**Pros:**
✅ Maximum flexibility (reflection, dynamic loading, plugins)  
✅ Smallest package size (IL is compact)  
✅ Cross-platform IL binaries  
✅ Fastest development iteration  

**Cons:**
❌ Highest cold start time  
❌ Unpredictable warm-up period  
❌ Larger memory footprint  

---

### ReadyToRun (R2R): Hybrid Approach

**How It Works:**
- Application ships with **both IL and precompiled native images**
- Native code for common paths; JIT for generics/edge cases
- Reduces initial compilation overhead
- Still requires full .NET runtime

**Characteristics:**
```
+------------------------------------------+
|  Application Start                       |
+------------------------------------------+
|  1. Load R2R + IL assemblies             |
|  2. Initialize runtime (CLR)             |
|  3. Execute precompiled code             |
|  4. JIT only for generics/new types      |
+------------------------------------------+
```

**Pros:**
✅ Faster startup than Regular (33% improvement in this demo)  
✅ Minimal code changes required  
✅ Maintains most .NET flexibility  
✅ Falls back to JIT when needed  

**Cons:**
❌ Larger package (IL + native images)  
❌ Still requires managed runtime  
❌ Platform-specific R2R images  
❌ In this demo: higher warm latency than Regular  

---

### NativeAOT: Ahead-of-Time Compilation

**How It Works:**
- **Entire application compiled to native code** at build time
- **No JIT compiler** included in deployment
- **Aggressive trimming** removes unused code
- Single native executable (self-contained)

**Characteristics:**
```
+------------------------------------------+
|  Application Start                       |
+------------------------------------------+
|  1. Execute native binary directly       |
|  2. No runtime initialization overhead   |
|  3. No JIT compilation                   |
|  4. Predictable, consistent performance  |
+------------------------------------------+
```

**Pros:**
✅ **Fastest cold start** (4–7× faster than Regular)  
✅ **Lowest memory usage** (~40–50 MB savings)  
✅ **Predictable performance** (no JIT pauses)  
✅ **No managed runtime dependency** (works on `provided.al2023`)  
✅ **Deploy .NET 9/10 today** on Lambda (managed runtime only supports .NET 8)  

**Cons:**
❌ Larger package than Regular (but smaller than R2R in .NET 9/10)  
❌ **Reflection limitations** (requires source generators)  
❌ **No dynamic assembly loading**  
❌ Platform-specific binaries  
❌ Longer build times  

---

## 🚀 Why NativeAOT Matters

### The Serverless Challenge

In serverless environments like AWS Lambda:
- **Cold starts hurt user experience** and cost money
- **Memory usage directly impacts billing**
- **Initialization time is pure overhead**

Traditional .NET cold starts include:
1. Download package from S3
2. Extract to execution environment
3. **Initialize .NET runtime** ⏱️ (biggest cost)
4. **Load and JIT assemblies** ⏱️ (second biggest)
5. Execute your code

**NativeAOT eliminates steps 3 & 4 entirely.**

### Performance Impact Comparison

**Averaged results from multiple test runs:**

| Metric         | Regular  | R2R      | AOT (.NET 8-10)     |
|----------------|----------|----------|---------------------|
| **Cold Start** | 6680 ms  | 4389 ms  | **940–1447 ms** ⚡  |
| **Warm Avg**   | 91 ms    | 99 ms    | **14–19 ms** ⚡     |
| **Memory**     | 88-93 MB | 89-96 MB | **42–52 MB** ⚡     |
| **Package**    | 1.37 MB  | 3.36 MB  | 5.56–6.33 MB        |

### Key Insights

**Cold Start:**
- AOT is **~7× faster** than Regular (6680ms → 940ms)
- AOT is **~4× faster** than R2R (4389ms → 940ms)
- Sub-second cold starts enable sync APIs and better UX

**Warm Performance:**
- AOT is **~6× faster** than Regular on warm runs (91ms → 14ms)
- Consistent 14–19ms latency vs 91–99ms for Regular/R2R
- No JIT pauses or tier optimization delays
- Predictable tail latencies (max 180ms vs 599ms)

**Memory Efficiency:**
- AOT uses **~50% less memory** than Regular/R2R (42-52 MB vs 88-96 MB)
- Enables smaller Lambda memory allocation → lower cost
- Trimming removes entire runtime subsystems
- Lower memory footprint = better density for high-volume workloads

**Package Size Trade-off:**
- AOT packages are larger than Regular (5.56-6.33 MB vs 1.37 MB)
- But **runtime efficiency far outweighs size** for serverless
- .NET 9/10 AOT smaller than .NET 8 (improved trimming: 6.33 → 5.56 MB)
- Single native binary vs multiple IL assemblies

**Cost Impact:**
- **73% lower Lambda costs** with AOT for high-volume workloads
- Example: 10M requests/month = **$7.20/month savings** per function
- Faster cold starts = fewer timeouts and better SLA compliance

---

## 📦 Build & Packaging Deep Dive

### Project Configuration Comparison

#### Regular .NET Project (`.csproj`)
```xml
<PropertyGroup>
  <OutputType>Library</OutputType>
  <TargetFramework>net8.0</TargetFramework>
  <RuntimeIdentifier>linux-x64</RuntimeIdentifier>
  <PublishAot>false</PublishAot>
  <PublishReadyToRun>false</PublishReadyToRun>
</PropertyGroup>
```

#### ReadyToRun Project (`.csproj`)
```xml
<PropertyGroup>
  <OutputType>Library</OutputType>
  <TargetFramework>net8.0</TargetFramework>
  <RuntimeIdentifier>linux-x64</RuntimeIdentifier>
  <PublishAot>false</PublishAot>
  <PublishReadyToRun>true</PublishReadyToRun> <!-- Enable R2R -->
  <TrimMode>partial</TrimMode>
</PropertyGroup>
```

#### NativeAOT Project (`.csproj`)
```xml
<PropertyGroup>
  <OutputType>Exe</OutputType> <!-- Must be Exe -->
  <TargetFramework>net8.0</TargetFramework>
  <RuntimeIdentifiers>linux-x64</RuntimeIdentifiers>
  <PublishAot>true</PublishAot> <!-- Enable AOT -->
  <SelfContained>true</SelfContained>
  <StripSymbols>true</StripSymbols> <!-- Reduce size -->
  <TrimMode>partial</TrimMode>
  <InvariantGlobalization>true</InvariantGlobalization>
</PropertyGroup>
```

**Key Differences:**
- AOT requires `OutputType` = `Exe` (produces native executable)
- AOT uses `Amazon.Lambda.RuntimeSupport` (custom runtime bootstrap)
- Regular/R2R use `Amazon.Lambda.Annotations` (managed runtime)

### Build Commands

#### Regular/R2R Build
```bash
dotnet restore ./LambdaRegularDemo/LambdaRegularDemo.csproj
dotnet publish ./LambdaRegularDemo/LambdaRegularDemo.csproj \
  -c Release \
  -o /artifacts/publish

cd /artifacts/publish
zip -r LambdaRegularDemo-lambda.zip . -x '*.dbg' -x '*.xml' -x '*.pdb'
```

**Output:** Multiple IL assemblies + dependencies

#### NativeAOT Build
```bash
dotnet restore ./LambdaAOTDemo9/LambdaAOTDemo9.csproj
dotnet publish ./LambdaAOTDemo9/LambdaAOTDemo9.csproj \
  -c Release \
  -o /artifacts/publish

# Rename native binary to 'bootstrap' for custom runtime
mv /artifacts/publish/LambdaAOTDemo9 /artifacts/publish/bootstrap

cd /artifacts/publish
zip -r LambdaAOTDemo9-lambda.zip . -x '*.dbg' -x '*.xml' -x '*.pdb'
```

**Output:** Single native `bootstrap` executable

### Dockerfile Multi-Stage Build

This repo uses a containerized build to ensure consistent Linux binaries:

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

# Install native toolchain for AOT
RUN apt-get update && \
    apt-get install -y clang zlib1g-dev zip

WORKDIR /src
COPY . .

# Build AOT Lambda 9
RUN dotnet restore ./LambdaAOTDemo9/LambdaAOTDemo9.csproj && \
    dotnet publish ./LambdaAOTDemo9/LambdaAOTDemo9.csproj -c Release -o /artifacts/publish && \
    mv /artifacts/publish/LambdaAOTDemo9 /artifacts/publish/bootstrap && \
    cd /artifacts/publish && zip -r /artifacts/LambdaAOTDemo9-lambda.zip .

# Build R2R and Regular similarly...
```

**Why Docker:**
- Ensures Linux build on any development OS
- Includes required native dependencies (`clang`, `zlib1g-dev`)
- Reproducible builds across team

### Package Anatomy

#### Regular Package Contents
```
LambdaRegularDemo-lambda.zip
├── LambdaRegularDemo.dll          (IL assembly)
├── Shared.dll                      (IL assembly)
├── Amazon.Lambda.Core.dll          (IL assembly)
├── Amazon.Lambda.Serialization.SystemTextJson.dll
├── AWSSDK.DynamoDBv2.dll
├── LambdaRegularDemo.deps.json
└── LambdaRegularDemo.runtimeconfig.json
```

#### NativeAOT Package Contents
```
LambdaAOTDemo9-lambda.zip
├── bootstrap                       (single native executable, ~5-6 MB)
└── (no other files required!)
```

**Note:** AOT trims everything to a single binary—no separate assemblies, no runtime.

---

## 💻 Coding for AOT: Patterns & Best Practices

### Challenge: Reflection Limitations

NativeAOT uses static analysis at build time to determine what code is used. **Dynamic reflection breaks this.**

#### ❌ Problematic Patterns (Don't Use with AOT)

```csharp
// Dynamic type loading
Type t = Type.GetType("MyNamespace.MyClass");
var instance = Activator.CreateInstance(t);

// Assembly loading
Assembly asm = Assembly.Load("PluginAssembly");

// Runtime code generation
DynamicMethod method = new DynamicMethod(...);

// Reflection-based JSON serialization
JsonSerializer.Serialize(obj); // Uses reflection by default!
```

#### ✅ AOT-Friendly Patterns

### Solution 1: JSON Source Generation

**Problem:** `System.Text.Json` uses reflection by default.  
**Solution:** Use compile-time source generation.

**From this repo (`src/LambdaAOTDemo9/AOTJsonContext.cs`):**
```csharp
using System.Text.Json.Serialization;

namespace LambdaAOTDemo
{
    [JsonSerializable(typeof(Guid))]
    [JsonSerializable(typeof(Dictionary<string, string>))]
    public partial class AOTJsonContext : JsonSerializerContext
    {
    }
}
```

**Usage in code:**
```csharp
// ❌ DON'T: Uses reflection
var json = JsonSerializer.Serialize(input);

// ✅ DO: Uses source-generated code
var json = JsonSerializer.Serialize(input, AOTJsonContext.Default.DictionaryStringString);
```

**How it works:**
1. Compiler sees `[JsonSerializable]` attributes
2. Generates optimized serialization code at compile time
3. No reflection needed at runtime
4. Faster and trimming-safe

### Solution 2: Dependency Injection (Constructor Injection)

**From this repo (`src/LambdaAOTDemo9/Function.cs`):**
```csharp
public class Function
{
    private readonly IDynamoDBRepository _dynamoDBRepository;

    // Constructor injection - AOT-friendly
    public Function(IDynamoDBRepository dynamoDBRepository)
    {
        _dynamoDBRepository = dynamoDBRepository;
    }

    public async Task<Guid> FunctionHandler(Dictionary<string, string> input, ILambdaContext context)
    {
        using var cts = new CancellationTokenSource(context.RemainingTime);
        var id = await _dynamoDBRepository.CreateAsync(cts.Token);
        return id;
    }
}
```

**Startup configuration (`src/LambdaAOTDemo9/Startup.cs`):**
```csharp
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Register services explicitly
        SharedStartup.ConfigureServices(services);
    }
}
```

**Why this works:**
- Types are known at compile time
- No service location or dynamic type resolution
- Trimmer can see the dependency graph

### Solution 3: Avoiding Dynamic Assembly Loading

**❌ DON'T:**
```csharp
// Runtime plugin discovery
var pluginAssemblies = Directory.GetFiles(pluginPath, "*.dll")
    .Select(Assembly.LoadFrom);
```

**✅ DO:**
```csharp
// Compile-time registration
services.AddSingleton<IPlugin, ConcretePlugin1>();
services.AddSingleton<IPlugin, ConcretePlugin2>();
```

### Solution 4: Preserving Types with Attributes

If you **must** use reflection on specific types:

```csharp
using System.Diagnostics.CodeAnalysis;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
public class MyModel
{
    public string Name { get; set; }
    public int Value { get; set; }
}
```

This tells the trimmer: "Keep all public properties on this type."

### Trimming Warnings

AOT build will produce warnings for problematic code:

```
warning IL2026: Using member 'Type.GetType(string)' which has 'RequiresUnreferencedCodeAttribute' 
can break functionality when trimming application code.
```

**Fix these warnings before deploying!**

---

## 🔬 Real-World Examples from This Repo

### Example 1: Lambda Function Comparison

All three Lambda functions do the same thing:
1. Receive input dictionary
2. Create a DynamoDB record via `IDynamoDBRepository`
3. Return the created GUID

**Key Difference:** How they serialize and initialize.

#### Regular Function (uses Lambda Annotations)
```csharp
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace LambdaRegularDemo;

public class Function
{
    private readonly IDynamoDBRepository _dynamoDBRepository;

    public Function(IDynamoDBRepository dynamoDBRepository)
    {
        _dynamoDBRepository = dynamoDBRepository;
    }

    [LambdaFunction] // Managed runtime annotation
    public async Task<Guid> FunctionHandler(Dictionary<string, string> input, ILambdaContext context)
    {
        // Uses reflection-based serializer (OK for Regular)
        var id = await _dynamoDBRepository.CreateAsync(cts.Token);
        return id;
    }
}
```

#### AOT Function (uses Runtime Support)
```csharp
public class Function
{
    private readonly IDynamoDBRepository _dynamoDBRepository;

    public Function(IDynamoDBRepository dynamoDBRepository)
    {
        _dynamoDBRepository = dynamoDBRepository;
    }

    public async Task<Guid> FunctionHandler(Dictionary<string, string> input, ILambdaContext context)
    {
        // Uses source-generated serializer (required for AOT)
        context.Logger.LogInformation(
            JsonSerializer.Serialize(input, AOTJsonContext.Default.DictionaryStringString)
        );
        
        var id = await _dynamoDBRepository.CreateAsync(cts.Token);
        return id;
    }
}
```

**Main entry point for AOT (`Program.cs`):**
```csharp
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;

var handler = async (Dictionary<string, string> input, ILambdaContext context) =>
{
    await using var serviceProvider = CreateServiceProvider();
    var function = serviceProvider.GetRequiredService<Function>();
    return await function.FunctionHandler(input, context);
};

await LambdaBootstrapBuilder.Create(handler, new SourceGeneratorLambdaJsonSerializer<AOTJsonContext>())
    .Build()
    .RunAsync();
```

### Example 2: Shared Services Across All Modes

The `Shared` project contains `IDynamoDBRepository` used by all Lambda functions:

```csharp
namespace Shared.Services;

public interface IDynamoDBRepository
{
    Task<Guid> CreateAsync(CancellationToken cancellationToken);
}

public class DynamoDbRepository : IDynamoDBRepository
{
    private readonly IAmazonDynamoDB _dynamoDb;
    private readonly string _tableName;

    public DynamoDbRepository(IAmazonDynamoDB dynamoDb, IConfiguration configuration)
    {
        _dynamoDb = dynamoDb;
        _tableName = configuration["TABLE_NAME"] ?? throw new Exception("TABLE_NAME not set");
    }

    public async Task<Guid> CreateAsync(CancellationToken cancellationToken)
    {
        var id = Guid.NewGuid();
        var item = new Dictionary<string, AttributeValue>
        {
            ["Id"] = new AttributeValue { S = id.ToString() },
            ["CreatedAt"] = new AttributeValue { S = DateTime.UtcNow.ToString("O") }
        };

        await _dynamoDb.PutItemAsync(_tableName, item, cancellationToken);
        return id;
    }
}
```

**This code works unchanged across Regular, R2R, and AOT** because:
- Uses constructor injection (compile-time)
- No reflection in business logic
- Configuration via `IConfiguration` (standard pattern)

### Example 3: JSON Context Evolution

**Regular/R2R** can use default serializer:
```csharp
// RegularDemoJsonContext.cs - still uses source generation for consistency
[JsonSerializable(typeof(Dictionary<string, string>))]
public partial class RegularDemoJsonContext : JsonSerializerContext { }
```

**AOT** requires it:
```csharp
// AOTJsonContext.cs - must use source generation
[JsonSerializable(typeof(Guid))]
[JsonSerializable(typeof(Dictionary<string, string>))]
public partial class AOTJsonContext : JsonSerializerContext { }
```

**Lesson:** Even if you're not using AOT today, adopting source generation early makes future migration easier.

---

## 🌍 Deployment Strategies

### AWS Lambda Runtime Options

| Runtime           | Use Case        | .NET Version Support     |
|-------------------|-----------------|--------------------------|
| `dotnet8`         | Managed runtime | .NET 8 only              |
| `provided.al2023` | Custom runtime  | Any (via AOT)            |
| Container Image   | Full control    | Any (via AOT or managed) |

### Strategy 1: Managed Runtime (Regular/R2R)

**Upload:** Zip file with IL assemblies  
**Handler:** `Assembly::Namespace.Class::Method`  
**Runtime:** `dotnet8`

**Example CloudFormation:**
```yaml
LambdaRegularFunction:
  Type: AWS::Lambda::Function
  Properties:
    Runtime: dotnet8
    Handler: LambdaRegularDemo::LambdaRegularDemo.Function::FunctionHandler
    Code:
      S3Bucket: !Ref DeploymentBucket
      S3Key: LambdaRegularDemo-lambda.zip
    MemorySize: 512
    Timeout: 30
```

### Strategy 2: Custom Runtime with AOT (provided.al2023)

**Upload:** Zip file with native `bootstrap` executable  
**Handler:** Not used (binary handles requests directly)  
**Runtime:** `provided.al2023`

**Example CloudFormation:**
```yaml
LambdaAOTFunction:
  Type: AWS::Lambda::Function
  Properties:
    Runtime: provided.al2023
    Handler: bootstrap # Not used but required field
    Code:
      S3Bucket: !Ref DeploymentBucket
      S3Key: LambdaAOTDemo9-lambda.zip
    MemorySize: 256 # Can use less memory with AOT!
    Timeout: 30
```

**Why `bootstrap`:**
- Custom runtime expects executable named `bootstrap`
- Lambda runtime calls `./bootstrap` to start your handler
- Your AOT binary includes the Lambda runtime client (`Amazon.Lambda.RuntimeSupport`)

### Strategy 3: Container Image with AOT

**No .NET runtime layer needed!**

```dockerfile
FROM public.ecr.aws/lambda/provided:al2023

# Copy only the native binary
COPY --from=build /artifacts/publish/bootstrap ${LAMBDA_RUNTIME_DIR}/bootstrap

CMD ["bootstrap"]
```

**Benefits:**
- Deploy .NET 9, .NET 10, or future versions **today**
- Smaller image (no 200MB+ .NET runtime layer)
- Faster cold starts (no runtime initialization)

### Deployment Workflow (from this repo)

```bash
# 1. Build all Lambda packages via Docker
docker build -f src/Dockerfile -t aot-r2r-regular .

# 2. Extract artifacts from container
docker create --name temp aot-r2r-regular
docker cp temp:/artifacts ./build-output
docker rm temp

# 3. Deploy to AWS
aws s3 cp ./build-output/LambdaAOTDemo9-lambda.zip s3://my-bucket/
aws lambda update-function-code \
  --function-name my-aot-function \
  --s3-bucket my-bucket \
  --s3-key LambdaAOTDemo9-lambda.zip
```

---

## 📊 Performance Results: The Numbers Don't Lie

### Test Methodology

**Setup:**
- AWS Lambda on `provided.al2023` (AOT) and `dotnet8` (Regular/R2R)
- 512MB memory allocation for Regular/R2R; 256MB for AOT
- Same business logic (DynamoDB write via `IDynamoDBRepository`)
- Same input payload
- **Results averaged from multiple test runs** to account for AWS infrastructure variance

**Cold Start Test:**
- Invoke after ~10 minutes of inactivity
- Measure billed duration from CloudWatch logs

**Warm Run Test:**
- Invoke 100 times in quick succession
- Calculate average, min, max billed duration
- Measure max memory used across all invocations

### Detailed Results Table

**Averaged Performance Metrics from Multiple Test Runs:**

| Function       | .NET Ver | Runtime         | Pkg Size | Cold Start | Warm Avg | Warm Min | Warm Max   | Max Mem  |
|----------------|----------|-----------------|----------|------------|----------|----------|------------|----------|
| **Regular**    | 8        | dotnet8         | 1.37 MB  | 6680 ms    | 91 ms    | 17-22 ms | 201-599 ms | 88-93 MB |
| **ReadyToRun** | 8        | dotnet8         | 3.36 MB  | 4389 ms    | 99 ms    | 21-26 ms | 322-661 ms | 89-96 MB |
| **AOT**        | 8        | dotnet8         | 6.33 MB  | 1082 ms    | 18 ms    | 5 ms     | 178-180 ms | 49-52 MB |
| **AOT**        | 8        | provided.al2023 | 6.33 MB  | 1447 ms    | 19 ms    | 5-7 ms   | 143-180 ms | 46-48 MB |
| **AOT**        | 9        | dotnet8         | 5.92 MB  | 971 ms     | 14 ms    | 5 ms     | 101-102 ms | 47-49 MB |
| **AOT**        | 9        | provided.al2023 | 5.92 MB  | 1006 ms    | 19 ms    | 5-6 ms   | 108-109 ms | 43-46 MB |
| **AOT**        | 10       | dotnet8         | 5.56 MB  | 940 ms     | 17 ms    | 5 ms     | 109-120 ms | 45-48 MB |
| **AOT**        | 10       | provided.al2023 | 5.56 MB  | 951 ms     | 17 ms    | 5-6 ms   | 109-122 ms | 42-45 MB |

### Visual Comparison: Cold Start

```
Regular .NET 8:      ████████████████████████████████████████████████████████████████ 6680ms
ReadyToRun .NET 8:   ████████████████████████████████████████ 4389ms
AOT .NET 8:          ████████ 1082ms ⚡
AOT .NET 9:          ███████ 971ms ⚡
AOT .NET 10:         ██████ 940ms ⚡ (FASTEST)
```

**Improvement: 7.1× faster cold start (Regular → AOT .NET 10)**

### Visual Comparison: Warm Average

```
ReadyToRun .NET 8:   ████████████████████ 99ms
Regular .NET 8:      ████████████████████ 91ms
AOT .NET 8:          ███ 18ms ⚡
AOT .NET 9:          ██ 14ms ⚡ (FASTEST)
AOT .NET 10:         ███ 17ms ⚡
```

**Improvement: 6.5× faster warm runs (Regular → AOT .NET 9)**

### Visual Comparison: Memory Usage

```
ReadyToRun .NET 8:   ████████████████████ 89-96 MB
Regular .NET 8:      ███████████████████ 88-93 MB
AOT .NET 8:          ██████████ 46-52 MB ⚡
AOT .NET 9:          █████████ 43-49 MB ⚡
AOT .NET 10:         ████████ 42-48 MB ⚡ (LOWEST)
```

**Improvement: 52% less memory (Regular → AOT .NET 10)**

### Key Performance Insights

**Cold Start Performance:**
- AOT delivers **sub-second cold starts** across all .NET versions (940-1447ms)
- Regular .NET takes **6.7 seconds**, making it unsuitable for latency-sensitive APIs
- ReadyToRun provides **34% improvement** over Regular (4.4s vs 6.7s)
- AOT is **4-7× faster** than ReadyToRun, **6-7× faster** than Regular

**Warm Run Performance:**
- AOT consistently delivers **14-19ms average latency**
- Regular/R2R average **91-99ms**, with occasional spikes to 600ms+
- AOT's **predictable performance** (max 180ms) vs Regular's volatility (max 599ms)
- No JIT pauses in AOT = stable tail latencies

**Memory Efficiency:**
- AOT uses **42-52 MB**, Regular/R2R use **88-96 MB**
- **~50% memory savings** enable smaller Lambda allocations
- Lower memory = lower cost in serverless billing

**Version Evolution:**
- **.NET 9 AOT**: Best warm performance (14ms avg)
- **.NET 10 AOT**: Best cold start (940ms) and lowest memory (42 MB)
- Package size decreases: .NET 8 (6.33 MB) → .NET 9 (5.92 MB) → .NET 10 (5.56 MB)

### Performance Variance Note

⚠️ **Important:** Serverless performance varies based on:
- AWS region and availability zone
- Time of day and infrastructure load
- Lambda execution environment reuse
- Network conditions to DynamoDB

**These results represent averaged measurements from multiple test runs.** Individual runs may vary by ±5-15%. Always benchmark your specific workload in your target AWS region.

### Cost Implications

**Lambda pricing (us-east-1):**
- Requests: $0.20 per 1M requests
- Duration: $0.0000166667 per GB-second

**Monthly cost for 1M requests (avg duration, optimized memory):**

| Mode        | Avg Duration | Memory | GB-Seconds | Duration Cost | Total Cost |
|-------------|--------------|--------|------------|---------------|------------|
| Regular     | 91 ms        | 512 MB | 46,592     | $0.78         | **$0.98**  |
| ReadyToRun  | 99 ms        | 512 MB | 50,688     | $0.84         | **$1.04**  |
| AOT .NET 9  | 14 ms        | 256 MB | 3,584      | $0.06         | **$0.26**  |
| AOT .NET 10 | 17 ms        | 256 MB | 4,352      | $0.07         | **$0.27**  |

**Savings: 73% lower cost with AOT** 💰

**At 10M requests/month:**
- Regular: **$9.80**
- AOT .NET 9: **$2.60** → **Save $7.20/month (73%)**

**Additional cold start cost savings:**
- Fewer timeouts = happier users
- Lower latency = better SLA compliance
- Faster cold starts = better user experience on infrequent functions

### ReadyToRun Performance Note

**Observation:** In these tests, ReadyToRun showed competitive warm performance (99ms avg) with Regular .NET (91ms avg), contrary to some expectations of significantly worse performance.

**Why this matters:**
- R2R provides **34% faster cold starts** with minimal warm performance impact
- Good intermediate step for codebases not ready for AOT constraints
- Test runs showed R2R variance: first test 104ms avg, second test 94ms avg
- **Takeaway:** R2R is a viable optimization if AOT's reflection limits are blocking

---

## 🔄 Migration Guide

### Quick Migration Path

**Phase 1: Assessment**
- Enable trim analyzers: `<EnableTrimAnalyzer>true</EnableTrimAnalyzer>`
- Identify AOT blockers: reflection, dynamic loading, incompatible libraries
- Create compatibility matrix

**Phase 2: Code Preparation**
```csharp
// Add JSON source generation
[JsonSerializable(typeof(MyModel))]
public partial class AppJsonContext : JsonSerializerContext { }

// Use constructor injection
public MyService(IRepository repo) => _repo = repo;

// Explicit registration (no assembly scanning)
services.AddSingleton<IService, Service>();
```

**Phase 3: Pilot Function**
```xml
<!-- Update .csproj -->
<PropertyGroup>
  <OutputType>Exe</OutputType>
  <PublishAot>true</PublishAot>
  <SelfContained>true</SelfContained>
</PropertyGroup>
```

**Phase 4: Gradual Rollout**
- Deploy side-by-side (Regular + AOT)
- Use Lambda aliases for weighted traffic routing
- Monitor CloudWatch metrics
- Gradual shift: 10% → 25% → 50% → 100%

### Common Challenges

**Third-Party Library Issues:**
- Find AOT-compatible alternatives
- Isolate incompatible code to separate Lambda
- Check library roadmap for AOT support

**ReadyToRun as Stepping Stone:**
```xml
<!-- 34% faster cold starts, minimal code changes -->
<PublishReadyToRun>true</PublishReadyToRun>
<TrimMode>partial</TrimMode>
```

---

## 🤔 When to Choose Which Approach

### Quick Decision Guide

```
Need cold start < 1s? ────► AOT
Traffic > 1M/month? ──────► AOT
Budget-sensitive? ────────► AOT
Complex reflection? ──────► Regular or R2R
Plugin architecture? ─────► Regular
Rapid prototyping? ───────► Regular
```

### Use Case Matrix

| Scenario                  | Recommendation | Why                                        |
|---------------------------|----------------|--------------------------------------------|
| **User-facing APIs**      | ✅ AOT         | 7× faster cold start, 6× faster warm runs  |
| **Event processors**      | ✅ AOT         | Frequent cold starts, cost-efficient       |
| **Scheduled tasks**       | ✅ AOT         | Always cold start, predictable performance |
| **High volume (>10M/mo)** | ✅ AOT         | 73% cost savings + better UX               |
| **Plugin systems**        | ✅ Regular     | Requires dynamic assembly loading          |
| **Heavy ORM (EF Core)**   | ⚠️ Regular/R2R | EF not fully AOT-compatible yet            |
| **MVPs/Prototypes**       | ✅ Regular     | Fastest iteration, switch later            |
| **Long-running (>5 min)** | ✅ Regular     | JIT optimization benefits                  |
| **Migration testing**     | ⚠️ ReadyToRun  | 34% improvement, low risk                  |

### ROI Calculator

```
At 10M requests/month:
Regular:  $9.80/mo  | 6.7s cold  | 91ms warm
AOT:      $2.60/mo  | 940ms cold | 14ms warm
Savings:  $7.20/mo (73%) + better UX
```

**When AOT Makes Sense:**
- Latency-sensitive applications
- High traffic volume (cost savings compound)
- Modern .NET features needed (9/10 on Lambda today)
- Predictable performance required

**When to Stay Regular:**
- Heavy reflection usage
- Dynamic plugin architecture
- Rapid development phase
- Third-party dependencies not AOT-ready

---

## ❓ FAQ & Troubleshooting

### Top Questions

**Q: Will my code work with AOT?**  
A: Not automatically. Check for:
- ❌ `Type.GetType()`, `Activator.CreateInstance()`
- ❌ Reflection-based JSON serialization
- ❌ Assembly scanning/dynamic loading
- ✅ Use source generation and constructor injection

**Q: Can I use Entity Framework?**  
A: Limited support. Better alternatives:
- ✅ Dapper (fully AOT-compatible)
- ✅ ADO.NET (fully compatible)
- ⚠️ EF Core (partial support, avoid dynamic LINQ)

**Q: Build times too long?**  
A: AOT builds are 2-5× slower. Optimize with:
- Docker layer caching
- Incremental builds for development
- Parallel CI/CD builds

**Q: Can I mix AOT and Regular?**  
A: Yes! Common pattern:
```
User Request → AOT Lambda (fast API)
                    ↓ SQS
              Regular Lambda (complex processing)
```

### Common Errors

**`IL2026: Requires unreferenced code`**
```csharp
// Solution: Use source generation
[JsonSerializable(typeof(MyClass))]
public partial class MyJsonContext : JsonSerializerContext { }
```

**`Could not find 'bootstrap'`**
```bash
# Rename binary after publish
mv MyLambdaFunction bootstrap
zip -r lambda.zip bootstrap
```

**Cold start still slow (>2s)**
```yaml
# Check: VPC adds 3-10s!
VpcConfig: !Ref AWS::NoValue  # Remove if not needed
```

**Warm execution slow**
```csharp
// Reuse AWS clients (don't create each request)
private static readonly IAmazonDynamoDB _client = new AmazonDynamoDBClient();
```

### Performance Troubleshooting

**Verify AOT is enabled:**
```bash
# Should see single ~5-6MB 'bootstrap' file
ls -lh bin/Release/net9.0/linux-x64/publish/
```

**Add detailed logging:**
```csharp
context.Logger.LogInformation($"DI: {sw.ElapsedMilliseconds}ms");
context.Logger.LogInformation($"Logic: {sw.ElapsedMilliseconds}ms");
context.Logger.LogInformation($"AWS API: {sw.ElapsedMilliseconds}ms");
```

### Getting Help

**Resources:**
- [.NET AOT Discussions](https://github.com/dotnet/runtime/discussions/categories/native-aot)
- [AWS Lambda .NET Issues](https://github.com/aws/aws-lambda-dotnet/issues)
- [This Repo Issues](https://github.com/whitewAw/dotnet-lambda-aot-performance-comparison/issues)

**Include when posting:**
- `.csproj` configuration
- Full build warnings
- CloudWatch logs
- Minimal reproduction code

---

## 🎓 Key Takeaways

### 1. NativeAOT Delivers Real Performance Gains

**Not hype—measured results averaged from multiple test runs:**
- **7× faster cold starts** (6680ms → 940ms)
- **6× faster warm runs** (91ms → 14ms)
- **50% less memory** (93MB → 42-45MB)
- **73% lower Lambda costs** for high-volume workloads

**Quick response = dual benefit:**
- **Lower costs**: $72/month savings per function at 100M requests
- **Happier users**: 2,138 hours of waiting time eliminated at 100M requests
- **Better business metrics**: Higher conversion, lower churn, improved SLA compliance

### 2. You Can Run .NET 9/10 on Lambda Today

**Managed runtime lags .NET releases**, but AOT doesn't care:
- Deploy .NET 10 on Lambda via `provided.al2023`
- Use cutting-edge framework features
- No waiting for AWS runtime updates
- **.NET 10 AOT** shows best cold start (940ms) and lowest memory (42 MB)

### 3. AOT Requires Code Discipline

**Not a magic switch—requires refactoring:**
- Replace reflection with source generators
- Use constructor injection
- Avoid dynamic assembly loading
- Fix trimming warnings

**But the patterns are good anyway:** more testable, better performance even without AOT.

### 4. Start Preparing Now

**Even if you stay on Regular today:**
- Adopt JSON source generation (`[JsonSerializable]`)
- Prefer constructor injection
- Avoid `Type.GetType`, `Assembly.Load`
- Enable trim analyzers

**This makes future AOT migration trivial.**

### 5. ReadyToRun Is a Viable Middle Ground

**34% faster cold start** with minimal warm performance impact:
- Good migration step (4389ms vs 6680ms cold start)
- Test trimming compatibility
- Competitive warm performance (99ms avg, similar to Regular's 91ms)
- Incremental improvement without AOT constraints
- **But**: Still 300% more expensive than AOT and slower response times

### 6. Measure Your Workload

**Results vary by application and environment:**
- This demo: simple DynamoDB write
- Your app: different patterns, dependencies, AWS region
- Observed variance: ±5-15% between test runs
- Always benchmark your specific code in your target environment

### 7. The Future Is AOT

**.NET investment in NativeAOT:**
- .NET 8: Stable for console apps, minimal APIs
- .NET 9: Improved library support, smaller binaries (5.92 MB), best warm performance (14ms avg)
- .NET 10: Continued improvements, smallest binaries (5.56 MB), fastest cold start (940ms)
- Growing ecosystem support (AWS SDK, Azure SDK, etc.)
- Each version shows measurable improvements in size and performance

### 8. Performance Is Predictable

**AOT eliminates runtime variance:**
- No JIT warm-up period
- No tier compilation delays
- Max latencies: AOT ~180ms vs Regular ~599ms
- Stable, predictable response times for SLA compliance

### 9. User Experience Drives Business Value

**Every millisecond matters in web applications:**
- **Sub-100ms response** = instant feel, natural interaction
- **6.7s cold start** (Regular) = unacceptable, high abandonment
- **940ms cold start** (AOT) = acceptable for most use cases
- **14ms warm response** (AOT) = excellent, instant feel

**Business impact:**
- 100ms improvement = 1% revenue increase (Amazon study)
- 53% mobile users abandon >3s loads (Google)
- **Quick responses improve both operating costs AND customer satisfaction**
- At scale: thousands of hours of user time saved = competitive advantage

---

## 📚 Additional Resources

### Microsoft Documentation
- [Native AOT Deployment](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/)
- [Trim Self-Contained Deployments](https://learn.microsoft.com/en-us/dotnet/core/deploying/trimming/)
- [System.Text.Json Source Generation](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/source-generation)

### AWS Documentation
- [AWS Lambda for .NET](https://docs.aws.amazon.com/lambda/latest/dg/lambda-csharp.html)
- [Custom Runtimes](https://docs.aws.amazon.com/lambda/latest/dg/runtimes-custom.html)
- [Lambda Container Images](https://docs.aws.amazon.com/lambda/latest/dg/images-create.html)

### This Repository
- **Build:** `docker build -f src/Dockerfile -t aot-demo .`
- **Projects:** `LambdaAOTDemo8/9/10`, `LambdaReadyToRunDemo`, `LambdaRegularDemo`
- **Shared Code:** `src/Shared/` (AOT-compatible business logic)

---

## 🚀 Getting Started

### Clone and Build

```bash
# Clone the repository
git clone https://github.com/yourusername/dotnet-lambda-aot-performance-comparison.git
cd dotnet-lambda-aot-performance-comparison

# Build all Lambda packages using Docker
docker build -f src/Dockerfile -t aot-r2r-regular .

# Extract build artifacts
docker create --name temp aot-r2r-regular
docker cp temp:/artifacts ./build-output
docker rm temp
```

### Deploy to AWS Lambda

```bash
# Upload to S3
aws s3 cp ./build-output/LambdaAOTDemo9-lambda.zip s3://your-bucket/

# Create or update Lambda function
aws lambda create-function \
  --function-name my-aot-function \
  --runtime provided.al2023 \
  --handler bootstrap \
  --role arn:aws:iam::YOUR_ACCOUNT:role/lambda-role \
  --code S3Bucket=your-bucket,S3Key=LambdaAOTDemo9-lambda.zip \
  --memory-size 256
```

### Run Performance Tests

```bash
# Deploy the invoker function
aws lambda create-function \
  --function-name lambda-invoker \
  --runtime dotnet8 \
  --handler LambdaInvoker::LambdaInvoker.Function::FunctionHandler \
  --code S3Bucket=your-bucket,S3Key=LambdaInvoker-lambda.zip \
  --environment Variables="{TARGET_FUNCTIONS=arn:aws:lambda:...}"

# Invoke and measure performance
aws lambda invoke \
  --function-name lambda-invoker \
  --payload '["arn:aws:lambda:region:account:function:my-aot-function"]' \
  response.json
```

---

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request. Areas of interest:
- Additional .NET versions as they're released
- Performance comparisons on different AWS regions
- Alternative serverless platforms (Azure Functions, Google Cloud Functions)
- More complex business logic examples

---

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

---

## 🙏 Acknowledgments

- Built with [.NET](https://dotnet.microsoft.com/) and [AWS Lambda](https://aws.amazon.com/lambda/)
- Performance data collected using [CloudWatch Logs](https://aws.amazon.com/cloudwatch/)
- Inspired by the .NET community's push toward NativeAOT

---

*This presentation demonstrates measured results from real Lambda functions. Your mileage may vary based on workload, dependencies, and AWS configuration. Always benchmark your specific use case.*

**⭐ Star this repo if you found it helpful!**
