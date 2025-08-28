1️⃣ AspireHost (aspire-host/)

  Distributed application orchestration for services APIs

  AspireHost/
  ├── Features/
  │   ├── ServiceDiscovery/
  │   │   ├── ServiceDiscoveryConfiguration.cs
  │   │   ├── ServiceRegistration.cs
  │   │   └── ServiceDiscoveryTests.cs         # Integrated tests
  │   ├── ResourceOrchestration/
  │   │   ├── DatabaseResource.cs              # PostgreSQL
  │   │   ├── CacheResource.cs                 # Redis
  │   │   ├── MessagingResource.cs             # RabbitMQ
  │   │   ├── StorageResource.cs               # Azure Blob
  │   │   ├── ObservabilityResource.cs         # Prometheus/Grafana
  │   │   └── ResourceOrchestrationTests.cs    # Integrated tests
  │   ├── EnvironmentManagement/
  │   │   ├── DevelopmentEnvironment.cs        # Local containers
  │   │   ├── TestingEnvironment.cs            # In-memory resources
  │   │   ├── ProductionEnvironment.cs         # Azure Container Apps
  │   │   └── EnvironmentTests.cs              # Integrated tests
  │   └── HealthOrchestration/
  │       ├── DistributedHealthChecks.cs
  │       ├── ServiceHealthMonitoring.cs
  │       └── HealthOrchestrationTests.cs      # Integrated tests
  ├── Shared/
  │   ├── Extensions/
  │   │   ├── AspireExtensions.cs
  │   │   └── ResourceExtensions.cs
  │   ├── Configuration/
  │   │   ├── AspireConfiguration.cs
  │   │   └── EnvironmentConfiguration.cs
  │   └── Utilities/
  │       ├── ResourceHelpers.cs
  │       └── ConfigurationHelpers.cs
  ├── Properties/
  │   └── launchSettings.json
  ├── appsettings.json
  ├── appsettings.Development.json
  ├── appsettings.Testing.json
  ├── appsettings.Production.json
  ├── AspireHost.csproj
  └── Program.cs

  ---
  2️⃣ SharedPlatform (shared-platform/)

  All shared infrastructure and cross-cutting concerns

  SharedPlatform/
  ├── Features/
  │   ├── DomainPrimitives/                   # Core domain building blocks
  │   │   ├── Entities/
  │   │   │   ├── BaseEntity.cs
  │   │   │   ├── BaseAggregateRoot.cs
  │   │   │   ├── IAuditable.cs
  │   │   │   ├── ISoftDeletable.cs
  │   │   │   ├── IVersioned.cs
  │   │   │   └── DomainPrimitivesTests.cs
  │   │   ├── ValueObjects/
  │   │   │   ├── BaseValueObject.cs
  │   │   │   ├── EntityId.cs
  │   │   │   ├── Slug.cs
  │   │   │   ├── Email.cs
  │   │   │   ├── PhoneNumber.cs
  │   │   │   └── ValueObjectTests.cs
  │   │   ├── Specifications/
  │   │   │   ├── ISpecification.cs
  │   │   │   ├── BaseSpecification.cs
  │   │   │   ├── CompositeSpecification.cs
  │   │   │   ├── ExpressionSpecification.cs
  │   │   │   └── SpecificationTests.cs
  │   │   └── DomainEvents/
  │   │       ├── IDomainEvent.cs
  │   │       ├── BaseDomainEvent.cs
  │   │       ├── DomainEventDispatcher.cs
  │   │       └── DomainEventTests.cs
  │   ├── ResultHandling/                     # Comprehensive result patterns
  │   │   ├── Result.cs
  │   │   ├── ResultT.cs
  │   │   ├── Error.cs
  │   │   ├── ErrorType.cs
  │   │   ├── PagedResult.cs
  │   │   ├── OperationResult.cs
  │   │   ├── ResultExtensions.cs
  │   │   ├── ResultFluentExtensions.cs
  │   │   └── ResultHandlingTests.cs
  │   ├── DataAccess/                         # Unified data access patterns
  │   │   ├── EntityFramework/
  │   │   │   ├── BaseDbContext.cs
  │   │   │   ├── BaseEfRepository.cs
  │   │   │   ├── EfUnitOfWork.cs
  │   │   │   ├── EfQueryHandler.cs
  │   │   │   ├── EfSpecificationEvaluator.cs
  │   │   │   └── EntityFrameworkTests.cs
  │   │   ├── Dapper/
  │   │   │   ├── BaseDapperRepository.cs
  │   │   │   ├── DapperConnectionFactory.cs
  │   │   │   ├── DapperQueryHandler.cs
  │   │   │   ├── DapperExtensions.cs
  │   │   │   └── DapperTests.cs
  │   │   ├── Abstractions/
  │   │   │   ├── IRepository.cs
  │   │   │   ├── IQueryRepository.cs
  │   │   │   ├── ICommandRepository.cs
  │   │   │   ├── IUnitOfWork.cs
  │   │   │   └── IDbConnectionFactory.cs
  │   │   ├── Interceptors/
  │   │   │   ├── AuditInterceptor.cs
  │   │   │   ├── SoftDeleteInterceptor.cs
  │   │   │   ├── TimestampInterceptor.cs
  │   │   │   ├── DomainEventInterceptor.cs
  │   │   │   └── InterceptorTests.cs
  │   │   └── Configuration/
  │   │       ├── DatabaseOptions.cs
  │   │       ├── ConnectionStringOptions.cs
  │   │       ├── DataAccessConfiguration.cs
  │   │       └── ConfigurationTests.cs
  │   ├── MedicalAudit/                       # Medical-grade audit system
  │   │   ├── Abstractions/
  │   │   │   ├── IAuditService.cs
  │   │   │   ├── IAuditRepository.cs
  │   │   │   └── IAuditContext.cs
  │   │   ├── Services/
  │   │   │   ├── MedicalAuditService.cs
  │   │   │   ├── AuditEventProcessor.cs
  │   │   │   └── AuditContextService.cs
  │   │   ├── Models/
  │   │   │   ├── AuditEntry.cs
  │   │   │   ├── AuditEvent.cs
  │   │   │   ├── AuditContext.cs
  │   │   │   ├── AuditSeverity.cs
  │   │   │   └── AuditAction.cs
  │   │   ├── Repository/
  │   │   │   ├── EfAuditRepository.cs
  │   │   │   └── AuditDbContext.cs
  │   │   ├── Configuration/
  │   │   │   ├── AuditOptions.cs
  │   │   │   └── AuditConfiguration.cs
  │   │   └── MedicalAuditTests.cs
  │   ├── Authentication/                     # Unified authentication system
  │   │   ├── Abstractions/
  │   │   │   ├── IAuthenticationService.cs
  │   │   │   ├── ITokenService.cs
  │   │   │   └── IUserContext.cs
  │   │   ├── Services/
  │   │   │   ├── JwtAuthenticationService.cs
  │   │   │   ├── JwtTokenService.cs
  │   │   │   ├── UserContextService.cs
  │   │   │   └── AnonymousAuthService.cs
  │   │   ├── Models/
  │   │   │   ├── AuthenticationResult.cs
  │   │   │   ├── TokenValidationResult.cs
  │   │   │   ├── UserPrincipal.cs
  │   │   │   └── AuthenticationContext.cs
  │   │   ├── Configuration/
  │   │   │   ├── AuthenticationOptions.cs
  │   │   │   ├── JwtOptions.cs
  │   │   │   └── EntraIdOptions.cs
  │   │   ├── Extensions/
  │   │   │   ├── AuthenticationExtensions.cs
  │   │   │   └── ClaimsPrincipalExtensions.cs
  │   │   └── AuthenticationTests.cs
  │   ├── Authorization/                      # Policy-based authorization
  │   │   ├── Abstractions/
  │   │   │   ├── IAuthorizationService.cs
  │   │   │   ├── IPermissionService.cs
  │   │   │   └── IPolicyProvider.cs
  │   │   ├── Services/
  │   │   │   ├── PolicyAuthorizationService.cs
  │   │   │   ├── PermissionService.cs
  │   │   │   └── RoleBasedPolicyProvider.cs
  │   │   ├── Policies/
  │   │   │   ├── PolicyBuilder.cs
  │   │   │   ├── RequirementHandler.cs
  │   │   │   ├── PermissionRequirement.cs
  │   │   │   └── RoleRequirement.cs
  │   │   ├── Models/
  │   │   │   ├── Permission.cs
  │   │   │   ├── Role.cs
  │   │   │   ├── PolicyResult.cs
  │   │   │   └── AuthorizationContext.cs
  │   │   ├── Configuration/
  │   │   │   ├── AuthorizationOptions.cs
  │   │   │   └── PolicyOptions.cs
  │   │   └── AuthorizationTests.cs
  │   ├── Security/                           # Comprehensive security
  │   │   ├── Cryptography/
  │   │   │   ├── IHashingService.cs
  │   │   │   ├── IEncryptionService.cs
  │   │   │   ├── HashingService.cs
  │   │   │   ├── EncryptionService.cs
  │   │   │   ├── KeyManagementService.cs
  │   │   │   └── CryptographyTests.cs
  │   │   ├── Headers/
  │   │   │   ├── SecurityHeadersService.cs
  │   │   │   ├── SecurityHeadersMiddleware.cs
  │   │   │   ├── ContentSecurityPolicy.cs
  │   │   │   └── SecurityHeadersTests.cs
  │   │   ├── DataProtection/
  │   │   │   ├── DataProtectionService.cs
  │   │   │   ├── DataProtectionExtensions.cs
  │   │   │   └── DataProtectionTests.cs
  │   │   └── Configuration/
  │   │       ├── SecurityOptions.cs
  │   │       └── SecurityConfiguration.cs
  │   ├── Observability/                      # Complete observability stack
  │   │   ├── Logging/
  │   │   │   ├── IStructuredLogger.cs
  │   │   │   ├── StructuredLogger.cs
  │   │   │   ├── LoggingExtensions.cs
  │   │   │   ├── LogEnricher.cs
  │   │   │   ├── LoggingConfiguration.cs
  │   │   │   └── LoggingTests.cs
  │   │   ├── Metrics/
  │   │   │   ├── IMetricsService.cs
  │   │   │   ├── ApplicationMetrics.cs
  │   │   │   ├── CustomMetrics.cs
  │   │   │   ├── MetricsCollector.cs
  │   │   │   ├── MetricsConfiguration.cs
  │   │   │   └── MetricsTests.cs
  │   │   ├── Tracing/
  │   │   │   ├── ITracingService.cs
  │   │   │   ├── TracingService.cs
  │   │   │   ├── ActivitySources.cs
  │   │   │   ├── TracingExtensions.cs
  │   │   │   ├── TracingConfiguration.cs
  │   │   │   └── TracingTests.cs
  │   │   └── HealthChecks/
  │   │       ├── IHealthCheckService.cs
  │   │       ├── CompositeHealthCheck.cs
  │   │       ├── DatabaseHealthCheck.cs
  │   │       ├── CacheHealthCheck.cs
  │   │       ├── MessagingHealthCheck.cs
  │   │       ├── HealthCheckExtensions.cs
  │   │       └── HealthCheckTests.cs
  │   ├── Messaging/                          # MassTransit messaging
  │   │   ├── Abstractions/
  │   │   │   ├── IMessageBus.cs
  │   │   │   ├── ICommandBus.cs
  │   │   │   ├── IQueryBus.cs
  │   │   │   └── IEventBus.cs
  │   │   ├── Services/
  │   │   │   ├── MassTransitMessageBus.cs
  │   │   │   ├── CommandBus.cs
  │   │   │   ├── QueryBus.cs
  │   │   │   └── EventBus.cs
  │   │   ├── Configuration/
  │   │   │   ├── MassTransitConfiguration.cs
  │   │   │   ├── RabbitMqConfiguration.cs
  │   │   │   ├── MessagingOptions.cs
  │   │   │   └── ConsumerConfiguration.cs
  │   │   ├── Patterns/
  │   │   │   ├── RetryPolicy.cs
  │   │   │   ├── CircuitBreakerPolicy.cs
  │   │   │   └── TimeoutPolicy.cs
  │   │   └── MessagingTests.cs
  │   ├── Caching/                            # Distributed caching
  │   │   ├── Abstractions/
  │   │   │   ├── ICacheService.cs
  │   │   │   ├── IDistributedCache.cs
  │   │   │   └── ICacheKeyGenerator.cs
  │   │   ├── Services/
  │   │   │   ├── RedisCacheService.cs
  │   │   │   ├── MemoryCacheService.cs
  │   │   │   ├── HybridCacheService.cs
  │   │   │   └── CacheKeyGenerator.cs
  │   │   ├── Configuration/
  │   │   │   ├── CacheOptions.cs
  │   │   │   ├── RedisOptions.cs
  │   │   │   └── CacheConfiguration.cs
  │   │   ├── Policies/
  │   │   │   ├── CachePolicy.cs
  │   │   │   ├── ExpirationPolicy.cs
  │   │   │   └── EvictionPolicy.cs
  │   │   └── CachingTests.cs
  │   ├── Validation/                         # FluentValidation system
  │   │   ├── Abstractions/
  │   │   │   ├── IValidationService.cs
  │   │   │   └── IValidatorProvider.cs
  │   │   ├── Services/
  │   │   │   ├── ValidationService.cs
  │   │   │   ├── ValidatorProvider.cs
  │   │   │   └── ValidationContextProvider.cs
  │   │   ├── Validators/
  │   │   │   ├── BaseValidator.cs
  │   │   │   ├── EntityValidator.cs
  │   │   │   ├── ValueObjectValidator.cs
  │   │   │   └── CommandValidator.cs
  │   │   ├── Extensions/
  │   │   │   ├── ValidationExtensions.cs
  │   │   │   ├── FluentValidationExtensions.cs
  │   │   │   └── ValidationResultExtensions.cs
  │   │   ├── Configuration/
  │   │   │   ├── ValidationOptions.cs
  │   │   │   └── ValidationConfiguration.cs
  │   │   └── ValidationTests.cs
  │   ├── Configuration/                      # Configuration management
  │   │   ├── Abstractions/
  │   │   │   ├── IConfigurationService.cs
  │   │   │   └── IOptionsProvider.cs
  │   │   ├── Services/
  │   │   │   ├── ConfigurationService.cs
  │   │   │   ├── OptionsProvider.cs
  │   │   │   ├── SecretManager.cs
  │   │   │   └── FeatureFlagService.cs
  │   │   ├── Options/
  │   │   │   ├── BaseOptions.cs
  │   │   │   ├── EnvironmentOptions.cs
  │   │   │   ├── PlatformOptions.cs
  │   │   │   └── FeatureFlags.cs
  │   │   ├── Providers/
  │   │   │   ├── AzureKeyVaultProvider.cs
  │   │   │   ├── EnvironmentProvider.cs
  │   │   │   └── JsonConfigurationProvider.cs
  │   │   └── ConfigurationTests.cs
  │   └── Testing/                            # Comprehensive testing utilities
  │       ├── Fixtures/
  │       │   ├── BaseTestFixture.cs
  │       │   ├── AspireTestFixture.cs
  │       │   ├── DatabaseTestFixture.cs
  │       │   ├── WebApplicationTestFixture.cs
  │       │   ├── AuthenticationTestFixture.cs
  │       │   └── TestFixtureTests.cs
  │       ├── Builders/
  │       │   ├── TestDataBuilder.cs
  │       │   ├── EntityTestBuilder.cs
  │       │   ├── ValueObjectTestBuilder.cs
  │       │   ├── CommandTestBuilder.cs
  │       │   ├── QueryTestBuilder.cs
  │       │   └── TestBuilderTests.cs
  │       ├── Generators/
  │       │   ├── BogusDataGenerator.cs
  │       │   ├── FsCheckGenerators.cs
  │       │   ├── PropertyBasedGenerators.cs
  │       │   ├── EntityGenerators.cs
  │       │   └── DataGeneratorTests.cs
  │       ├── Mocks/
  │       │   ├── MockServiceProvider.cs
  │       │   ├── MockRepository.cs
  │       │   ├── MockMessageBus.cs
  │       │   ├── MockAuthenticationService.cs
  │       │   └── MockTests.cs
  │       ├── Utilities/
  │       │   ├── TestHelpers.cs
  │       │   ├── AssertionHelpers.cs
  │       │   ├── DatabaseTestHelpers.cs
  │       │   ├── HttpTestHelpers.cs
  │       │   ├── AuthTestHelpers.cs
  │       │   └── TestUtilityTests.cs
  │       ├── Extensions/
  │       │   ├── TestExtensions.cs
  │       │   ├── MockExtensions.cs
  │       │   ├── FixtureExtensions.cs
  │       │   ├── AssertionExtensions.cs
  │       │   └── TestExtensionTests.cs
  │       └── Configuration/
  │           ├── TestConfiguration.cs
  │           ├── TestSettings.cs
  │           └── TestEnvironmentSetup.cs
  ├── Shared/
  │   ├── Extensions/
  │   │   ├── ServiceCollectionExtensions.cs  # Platform DI registration
  │   │   ├── WebApplicationExtensions.cs     # Platform middleware pipeline
  │   │   ├── ConfigurationExtensions.cs      # Configuration helpers
  │   │   └── PlatformExtensions.cs           # Platform-wide extensions
  │   ├── Constants/
  │   │   ├── ErrorCodes.cs
  │   │   ├── SystemConstants.cs
  │   │   ├── ConfigurationKeys.cs
  │   │   ├── PolicyNames.cs
  │   │   └── EventNames.cs
  │   ├── Exceptions/
  │   │   ├── PlatformException.cs
  │   │   ├── DomainException.cs
  │   │   ├── BusinessRuleException.cs
  │   │   ├── ValidationException.cs
  │   │   ├── AuthenticationException.cs
  │   │   ├── AuthorizationException.cs
  │   │   └── InfrastructureException.cs
  │   └── Utilities/
  │       ├── DateTimeProvider.cs
  │       ├── GuidProvider.cs
  │       ├── StringHelpers.cs
  │       ├── CollectionHelpers.cs
  │       └── ExpressionHelpers.cs
  └── SharedPlatform.csproj

  ---
  3️⃣ ApiGateway (api-gateway/)

  Unified gateway for services public and admin APIs

  ApiGateway/
  ├── Features/
  │   ├── Routing/                            # Services API routing
  │   │   ├── IRoutingService.cs
  │   │   ├── YarpRoutingService.cs
  │   │   ├── RouteConfiguration.cs
  │   │   ├── PublicRouteProvider.cs          # Services public API routes
  │   │   ├── AdminRouteProvider.cs           # Services admin API routes
  │   │   ├── RouteTransformation.cs
  │   │   ├── LoadBalancing.cs
  │   │   └── RoutingTests.cs
  │   ├── RateLimiting/                       # Configuration-driven rate limiting
  │   │   ├── IRateLimitingService.cs
  │   │   ├── RateLimitingService.cs
  │   │   ├── RateLimitingMiddleware.cs
  │   │   ├── IpBasedRateLimiter.cs          # For public APIs (1000/min)
  │   │   ├── UserBasedRateLimiter.cs        # For admin APIs (100/min)
  │   │   ├── RedisRateLimitStore.cs
  │   │   ├── RateLimitConfiguration.cs
  │   │   ├── RateLimitPolicies.cs
  │   │   └── RateLimitingTests.cs
  │   ├── Authentication/                     # Multi-strategy authentication
  │   │   ├── IAuthenticationStrategy.cs
  │   │   ├── AuthenticationMiddleware.cs
  │   │   ├── AnonymousStrategy.cs           # For public APIs
  │   │   ├── EntraIdStrategy.cs             # For admin APIs
  │   │   ├── JwtStrategy.cs                 # For service-to-service
  │   │   ├── AuthenticationConfiguration.cs
  │   │   ├── AuthenticationPolicies.cs
  │   │   └── AuthenticationTests.cs
  │   ├── Authorization/                      # Policy-based authorization
  │   │   ├── IAuthorizationStrategy.cs
  │   │   ├── AuthorizationMiddleware.cs
  │   │   ├── PublicAuthorizationStrategy.cs  # Always allow
  │   │   ├── AdminAuthorizationStrategy.cs   # RBAC
  │   │   ├── AuthorizationPolicies.cs
  │   │   ├── PermissionValidation.cs
  │   │   └── AuthorizationTests.cs
  │   ├── Cors/                               # Environment-specific CORS
  │   │   ├── ICorsService.cs
  │   │   ├── CorsService.cs
  │   │   ├── CorsMiddleware.cs
  │   │   ├── PublicCorsPolicy.cs
  │   │   ├── AdminCorsPolicy.cs
  │   │   ├── CorsConfiguration.cs
  │   │   └── CorsTests.cs
  │   ├── Security/                           # Gateway security
  │   │   ├── SecurityHeadersMiddleware.cs
  │   │   ├── SecurityPolicies.cs
  │   │   ├── ContentSecurityPolicy.cs
  │   │   ├── HstsPolicy.cs
  │   │   ├── SecurityConfiguration.cs
  │   │   └── SecurityTests.cs
  │   ├── Observability/                      # Gateway-specific observability
  │   │   ├── RequestLoggingMiddleware.cs
  │   │   ├── MetricsCollectionMiddleware.cs
  │   │   ├── TracingMiddleware.cs
  │   │   ├── GatewayMetrics.cs
  │   │   ├── ObservabilityConfiguration.cs
  │   │   └── ObservabilityTests.cs
  │   ├── HealthChecks/                       # Gateway health monitoring
  │   │   ├── GatewayHealthCheck.cs
  │   │   ├── DownstreamHealthCheck.cs
  │   │   ├── RedisHealthCheck.cs
  │   │   ├── LoadBalancerHealthCheck.cs
  │   │   ├── HealthCheckConfiguration.cs
  │   │   └── HealthCheckTests.cs
  │   └── ErrorHandling/                      # Gateway error management
  │       ├── ErrorHandlingMiddleware.cs
  │       ├── GatewayErrorHandler.cs
  │       ├── ErrorResponseFormatter.cs
  │       ├── ErrorConfiguration.cs
  │       └── ErrorHandlingTests.cs
  ├── Shared/
  │   ├── Abstractions/
  │   │   ├── IGatewayService.cs
  │   │   ├── IRouteProvider.cs
  │   │   ├── IMiddlewareProvider.cs
  │   │   └── IGatewayConfiguration.cs
  │   ├── Models/
  │   │   ├── GatewayRequest.cs
  │   │   ├── GatewayResponse.cs
  │   │   ├── RouteDefinition.cs
  │   │   ├── PolicyDefinition.cs
  │   │   └── GatewayContext.cs
  │   ├── Configuration/
  │   │   ├── GatewayOptions.cs
  │   │   ├── PublicGatewayOptions.cs        # Public API configuration
  │   │   ├── AdminGatewayOptions.cs         # Admin API configuration
  │   │   ├── YarpOptions.cs
  │   │   └── MiddlewareOptions.cs
  │   ├── Extensions/
  │   │   ├── ServiceCollectionExtensions.cs
  │   │   ├── WebApplicationExtensions.cs
  │   │   ├── YarpExtensions.cs
  │   │   ├── PolicyExtensions.cs
  │   │   └── ConfigurationExtensions.cs
  │   ├── Services/
  │   │   ├── GatewayService.cs
  │   │   ├── ConfigurationProvider.cs
  │   │   ├── MiddlewarePipeline.cs
  │   │   └── GatewayContextService.cs
  │   └── Utilities/
  │       ├── RequestHelpers.cs
  │       ├── ResponseHelpers.cs
  │       ├── PolicyHelpers.cs
  │       └── ConfigurationHelpers.cs
  ├── Properties/
  │   └── launchSettings.json
  ├── appsettings.json
  ├── appsettings.Development.json
  ├── appsettings.Testing.json
  ├── appsettings.Production.json
  ├── yarp.Development.json                   # Development routing
  ├── yarp.Production.json                    # Production routing
  ├── ApiGateway.csproj
  └── Program.cs

  ---
  4️⃣ ServicesDomain (services-domain/)

  Services public and admin APIs with vertical slice architecture

  ServicesDomain/
  ├── Features/
  │   ├── ServiceManagement/                  # Core service domain
  │   │   ├── Domain/
  │   │   │   ├── Entities/
  │   │   │   │   └── Service.cs              # Main aggregate root with SharedPlatform integration
  │   │   │   ├── ValueObjects/
  │   │   │   │   ├── ServiceId.cs
  │   │   │   │   ├── ServiceTitle.cs
  │   │   │   │   ├── Description.cs          # Renamed from ShortDescription for schema clarity
  │   │   │   │   ├── PublishingStatus.cs     # Enum-based status with validation
  │   │   │   │   ├── ServiceSlug.cs
  │   │   │   │   ├── LongDescriptionUrl.cs
  │   │   │   │   └── DeliveryMode.cs
  │   │   │   └── Repository/
  │   │   │       └── IServiceRepository.cs   # Repository interface
  │   ├── CategoryManagement/                 # Category domain
  │   │   ├── Domain/
  │   │   │   ├── Entities/
  │   │   │   │   ├── ServiceCategory.cs      # Full SharedPlatform integration (BaseEntity, IAuditable, ISoftDeletable)
  │   │   │   │   └── FeaturedCategory.cs     # Medical-grade audit entity
  │   │   │   └── ValueObjects/
  │   │   │       └── ServiceCategoryId.cs   # Consistent value object pattern
  │   ├── GetService/                         # Public API: Get single service [IMPLEMENTED - TDD GREEN]
  │   │   ├── GetServiceQuery.cs              # CQRS query with validation
  │   │   ├── GetServiceHandler.cs            # Handler with medical audit, caching, LoggerMessage delegates
  │   │   ├── GetServiceEndpoint.cs           # Minimal API endpoint with error handling
  │   │   ├── GetServiceResponse.cs           # Response DTO
  │   │   └── GetServiceValidator.cs          # FluentValidation
  │   ├── GetServices/                        # Public API: List services
  │   │   ├── GetServicesQuery.cs
  │   │   ├── GetServicesHandler.cs
  │   │   ├── GetServicesEndpoint.cs
  │   │   ├── GetServicesResponse.cs
  │   │   ├── GetServicesRepository.cs        # Dapper with paging
  │   │   ├── GetServicesCaching.cs
  │   │   ├── GetServicesValidation.cs
  │   │   └── GetServicesTests.cs
  │   ├── GetServiceBySlug/                   # Public API: Get by slug
  │   │   ├── GetServiceBySlugQuery.cs
  │   │   ├── GetServiceBySlugHandler.cs
  │   │   ├── GetServiceBySlugEndpoint.cs
  │   │   ├── GetServiceBySlugResponse.cs
  │   │   ├── GetServiceBySlugRepository.cs   # Dapper with slug indexing
  │   │   ├── GetServiceBySlugCaching.cs
  │   │   ├── GetServiceBySlugValidation.cs
  │   │   └── GetServiceBySlugTests.cs
  │   ├── SearchServices/                     # Public API: Search services
  │   │   ├── SearchServicesQuery.cs
  │   │   ├── SearchServicesHandler.cs
  │   │   ├── SearchServicesEndpoint.cs
  │   │   ├── SearchServicesResponse.cs
  │   │   ├── SearchServicesRepository.cs     # Dapper with full-text search
  │   │   ├── SearchServicesCaching.cs
  │   │   ├── SearchServicesValidation.cs
  │   │   └── SearchServicesTests.cs
  │   ├── GetServiceCategories/               # Public API: List categories
  │   │   ├── GetCategoriesQuery.cs
  │   │   ├── GetCategoriesHandler.cs
  │   │   ├── GetCategoriesEndpoint.cs
  │   │   ├── GetCategoriesResponse.cs
  │   │   ├── GetCategoriesRepository.cs      # Dapper with hierarchy
  │   │   ├── GetCategoriesCaching.cs
  │   │   ├── GetCategoriesValidation.cs
  │   │   └── GetCategoriesTests.cs
  │   ├── CreateService/                      # Admin API: Create service
  │   │   ├── CreateServiceCommand.cs
  │   │   ├── CreateServiceConsumer.cs        # MassTransit consumer
  │   │   ├── CreateServiceHandler.cs
  │   │   ├── CreateServiceEndpoint.cs        # Minimal API with auth
  │   │   ├── CreateServiceRequest.cs
  │   │   ├── CreateServiceResponse.cs
  │   │   ├── CreateServiceRepository.cs      # EF Core implementation
  │   │   ├── CreateServiceValidation.cs      # FluentValidation
  │   │   ├── CreateServiceAuthorization.cs   # Permission checks
  │   │   ├── CreateServiceAudit.cs           # Medical-grade audit
  │   │   └── CreateServiceTests.cs           # Complete feature tests
  │   ├── UpdateService/                      # Admin API: Update service
  │   │   ├── UpdateServiceCommand.cs
  │   │   ├── UpdateServiceConsumer.cs
  │   │   ├── UpdateServiceHandler.cs
  │   │   ├── UpdateServiceEndpoint.cs
  │   │   ├── UpdateServiceRequest.cs
  │   │   ├── UpdateServiceResponse.cs
  │   │   ├── UpdateServiceRepository.cs      # EF Core with change tracking
  │   │   ├── UpdateServiceValidation.cs
  │   │   ├── UpdateServiceAuthorization.cs
  │   │   ├── UpdateServiceAudit.cs
  │   │   └── UpdateServiceTests.cs
  │   ├── DeleteService/                      # Admin API: Delete service
  │   │   ├── DeleteServiceCommand.cs
  │   │   ├── DeleteServiceConsumer.cs
  │   │   ├── DeleteServiceHandler.cs
  │   │   ├── DeleteServiceEndpoint.cs
  │   │   ├── DeleteServiceRequest.cs
  │   │   ├── DeleteServiceResponse.cs
  │   │   ├── DeleteServiceRepository.cs      # Soft delete with EF Core
  │   │   ├── DeleteServiceValidation.cs
  │   │   ├── DeleteServiceAuthorization.cs
  │   │   ├── DeleteServiceAudit.cs
  │   │   └── DeleteServiceTests.cs
  │   ├── BulkOperations/                     # Admin API: Bulk operations
  │   │   ├── BulkUpdateCommand.cs
  │   │   ├── BulkUpdateConsumer.cs
  │   │   ├── BulkUpdateHandler.cs
  │   │   ├── BulkUpdateEndpoint.cs
  │   │   ├── BulkUpdateRequest.cs
  │   │   ├── BulkUpdateResponse.cs
  │   │   ├── BulkUpdateRepository.cs         # Batch EF Core operations
  │   │   ├── BulkUpdateValidation.cs
  │   │   ├── BulkUpdateAuthorization.cs
  │   │   ├── BulkUpdateAudit.cs
  │   │   └── BulkOperationTests.cs
  │   ├── PublishServices/                    # Admin API: Publish services
  │   │   ├── PublishCommand.cs
  │   │   ├── PublishConsumer.cs
  │   │   ├── PublishHandler.cs
  │   │   ├── PublishEndpoint.cs
  │   │   ├── PublishRequest.cs
  │   │   ├── PublishResponse.cs
  │   │   ├── PublishRepository.cs
  │   │   ├── PublishValidation.cs
  │   │   ├── PublishAuthorization.cs
  │   │   ├── PublishAudit.cs
  │   │   └── PublishTests.cs
  │   ├── ArchiveServices/                    # Admin API: Archive services
  │   │   ├── ArchiveCommand.cs
  │   │   ├── ArchiveConsumer.cs
  │   │   ├── ArchiveHandler.cs
  │   │   ├── ArchiveEndpoint.cs
  │   │   ├── ArchiveRequest.cs
  │   │   ├── ArchiveResponse.cs
  │   │   ├── ArchiveRepository.cs
  │   │   ├── ArchiveValidation.cs
  │   │   ├── ArchiveAuthorization.cs
  │   │   ├── ArchiveAudit.cs
  │   │   └── ArchiveTests.cs
  │   └── HealthChecks/                       # Services API health monitoring
  │       ├── ServicesApiHealthCheck.cs
  │       ├── DatabaseHealthCheck.cs
  │       ├── CacheHealthCheck.cs
  │       ├── MessagingHealthCheck.cs
  │       └── HealthCheckTests.cs
  ├── Shared/
  │   ├── Infrastructure/
  │   │   ├── Data/
  │   │   │   ├── Configurations/              # EF Core .NET 9 configuration patterns
  │   │   │   │   ├── ServiceConfiguration.cs  # Main service entity config
  │   │   │   │   ├── ServiceAuditConfiguration.cs # Medical audit config
  │   │   │   │   ├── ServiceCategoryConfiguration.cs # Category entity config
  │   │   │   │   ├── ServiceCategoryAuditConfiguration.cs # Category audit config
  │   │   │   │   └── FeaturedCategoryConfiguration.cs # Featured category config
  │   │   │   ├── ServicesDbContext.cs        # EF Core context with medical audit and SHA256 hashing
  │   │   │   ├── IServicesDbContext.cs       # Context interface
  │   │   │   ├── ServiceAudit.cs             # Medical audit entity (matches services_audit table)
  │   │   │   ├── ServiceCategoryAudit.cs     # Category audit entity (matches service_categories_audit table)
  │   │   │   ├── ServicesConfiguration.cs    # Configuration registration
  │   │   │   ├── DapperConnectionFactory.cs  # Dapper factory for public APIs
  │   │   │   └── DatabaseMigrations.cs       # Migration management
  │   │   ├── Repositories/
  │   │   │   ├── EfServiceRepository.cs      # EF Core for admin APIs with ConfigureAwait patterns
  │   │   │   ├── DapperServiceRepository.cs  # Dapper for public APIs (future)
  │   │   │   ├── EfCategoryRepository.cs     # Category EF Core repository (future)
  │   │   │   └── DapperCategoryRepository.cs # Category Dapper repository (future)
  │   │   ├── Services/
  │   │   │   ├── ServiceCacheService.cs
  │   │   │   ├── ServiceNotificationService.cs
  │   │   │   ├── ServiceSearchService.cs
  │   │   │   └── ServiceAuditService.cs
  │   │   ├── Middleware/
  │   │   │   ├── PublicApiMiddleware.cs      # Public API pipeline
  │   │   │   ├── AdminApiMiddleware.cs       # Admin API pipeline
  │   │   │   ├── ExceptionHandlingMiddleware.cs
  │   │   │   ├── RequestLoggingMiddleware.cs
  │   │   │   ├── ValidationMiddleware.cs
  │   │   │   └── AuditMiddleware.cs
  │   │   ├── MassTransit/
  │   │   │   ├── MassTransitConfiguration.cs
  │   │   │   ├── ServicesMassTransitExtensions.cs
  │   │   │   ├── ConsumerConfiguration.cs
  │   │   │   └── ProducerConfiguration.cs
  │   │   └── ExternalServices/
  │   │       ├── IExternalServiceClient.cs
  │   │       ├── NoOpExternalServiceClient.cs
  │   │       └── ServiceIntegrationClient.cs
  │   ├── Models/
  │   │   ├── ServiceDto.cs                   # Shared DTOs
  │   │   ├── ServiceCategoryDto.cs
  │   │   ├── ServiceSearchResult.cs
  │   │   ├── ServiceListItem.cs
  │   │   ├── ServiceDetail.cs
  │   │   └── PaginatedResult.cs
  │   ├── Contracts/                          # API contracts
  │   │   ├── Queries/
  │   │   │   ├── IServiceQueries.cs
  │   │   │   └── ICategoryQueries.cs
  │   │   ├── Commands/
  │   │   │   ├── IServiceCommands.cs
  │   │   │   └── ICategoryCommands.cs
  │   │   └── Events/
  │   │       ├── IServiceEvents.cs
  │   │       └── ICategoryEvents.cs
  │   ├── Configuration/
  │   │   ├── ServicesOptions.cs
  │   │   ├── PublicApiOptions.cs             # Public API config
  │   │   ├── AdminApiOptions.cs              # Admin API config
  │   │   ├── DatabaseOptions.cs
  │   │   ├── CacheOptions.cs
  │   │   ├── SearchOptions.cs
  │   │   └── MessagingOptions.cs
  │   ├── Extensions/
  │   │   ├── ServiceCollectionExtensions.cs
  │   │   ├── WebApplicationExtensions.cs
  │   │   ├── DbContextExtensions.cs
  │   │   ├── MassTransitExtensions.cs
  │   │   └── ValidationExtensions.cs
  │   └── Utilities/
  │       ├── ServiceHelpers.cs
  │       ├── CategoryHelpers.cs
  │       ├── SlugGenerator.cs
  │       ├── SearchIndexer.cs
  │       └── CacheKeyGenerator.cs
  ├── Properties/
  │   └── launchSettings.json
  ├── appsettings.json
  ├── appsettings.Development.json
  ├── appsettings.Testing.json
  ├── appsettings.Production.json
  ├── ServicesDomain.csproj
  ├── Program.cs                              # Unified API with feature flags
  └── SERVICES-SCHEMA.md                     # Database schema specification

  ServicesDomain.Tests/                       # Separate test project for cohesion
  ├── Features/                              # Mirror main project structure
  │   ├── GetService/
  │   │   └── GetServiceTests.cs             # Comprehensive unit tests (14 passing)
  │   ├── ServiceManagement/
  │   │   └── ServiceManagementTests.cs      # Domain tests
  │   ├── CategoryManagement/
  │   │   └── CategoryManagementTests.cs     # Category domain tests
  │   ├── GetServiceBySlug/
  │   │   └── GetServiceBySlugTests.cs
  │   ├── GetServices/
  │   │   └── GetServicesTests.cs
  │   ├── GetServiceCategories/
  │   │   └── GetCategoriesTests.cs
  │   ├── SearchServices/
  │   │   └── SearchServicesTests.cs
  │   ├── CreateService/
  │   │   └── CreateServiceTests.cs
  │   ├── UpdateService/
  │   │   └── UpdateServiceTests.cs
  │   ├── DeleteService/
  │   │   └── DeleteServiceTests.cs
  │   ├── BulkOperations/
  │   │   └── BulkOperationTests.cs
  │   ├── PublishServices/
  │   │   └── PublishTests.cs
  │   ├── ArchiveServices/
  │   │   └── ArchiveTests.cs
  │   └── HealthChecks/
  │       └── HealthCheckTests.cs
  ├── Shared/
  │   └── TestFixtures/                      # Shared test utilities (future)
  └── ServicesDomain.Tests.csproj            # Test-specific dependencies