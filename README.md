# 3ai.solutions.SupabaseWrapper

## Installation

Services:

```csharp
builder.Services.Configure<SupabaseOptions>(builder.Configuration.GetSection("SupabaseOptions"));
builder.Services.AddScoped<SupabaseRepo>();
builder.Services.AddScoped<SupabaseService>();
```

Endpoints:

```csharp
app.MapSupabaseEndpoints();
```
