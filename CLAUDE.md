


- IMPORTANT AXIOM RULE TO FOLLOW : outside of unit tests , consider mocks and stubs the worst architectural anti pattern ( stop metioning the fact that you are using real implmentations , this is explicit in the implementation , keep the naming professional )

- IMPORTANT AXIOM RULE TO FOLLOW : we should not create new files outside of our current folders and files structure ( this is deliberate ) . ensure you stop and ask for permission if you believe we should alter our folder and files structure . our @international-center-website/STRUCTURE.md file should always reflect the current architectural folder and files structure trees

- IMPORTANT AXIOM RULE TO FOLLOW : do not edit any markdown files without permission . ensure you stop and ask for permission and provide your reasoning

- IMPORTANT AXIOM RULE TO FOLLOW : Our database schemas should exactly match that in our [name]-SCHEMA.md markdown files in the respective domains


- note : we will not work on the public and admin website frontend at the moment


[ important project related rules to follow ]

# development workflow

- test-driven development ( red phase , green phase , refactor phase ) ( tests drive and validate the design of our architecture ) ( creating new methods from refactoring should not require us to write new tests , this violates the contract-first testing principle ) ( you are allowed to modify the project and tests implementations as you see fit , since project and/or tests abstractions and/or implementations sometimes need to be updated ) ( when planning a new TDD cycle , provide a list of all the files you intend to edit and what you intend to do in each phase )


# architecture patterns

- dotnet aspire for distributed application orchestration

- cohesion over coupling
- best practices in our stack
- microsoft documentation recommended patterns

- synchronous and asynchronous patterns were appropriate

- configuration-based
- dependency inversion ( interfaces for variable concerns ) ( concrete types for stable concerns )

- cqrs pattern
- repository pattern
- specification pattern
- event sourcing ( for audit requirements )

- result type for expected failures ( business rules violations , so forth )
- exceptions for unexpected failures ( infrastructure failures , so forth )

- warnings as errors
- options pattern ( register the options class directly with '.Value' to avoid having to use '.Value' everywhere )
- singletons can only depend on singletons

# architecture layers

- lower layers should not depend on nor be aware of higher layers
- the lowest layer is the infrastructure layer
- the highest layer is the frontend
- services can be between any layers

# architecture top level structure

# public website

- astro
- vue
- tailwind
- shadcn-vue
- vite
- vitest
- pinia
- bun runtime

- public api gateway for dynamic data

- do not use react
- do not do UI design testing

# backend

- dotnet 9
- ef core for admin apis repositories
- dapper for public apis repositories
- minimal apis
- fluentvalidations
- mediatr
- masstransit

- xunit
- moq
- bogus
- FsCheck
- do not use FluentAssertion

- enable 'ValidateOnBuild'
- remove the kestrel server header from endpoints ( 'AddServerHeader' option of the kestrel webhost )

## package management

- central package management ( 'Directory.Packages.props' at the root defines packages and versions ) ( omit version attribute in 'PackageReference' )

## environment configuration

- development environment ( local environment ) ( session containers ) ( pdAdmin enabled )

- testing environemnt ( automated local environemnt tests ) ( bypass secrets configuration ) ( in-memory resources ) ( minimal authenticated for integration test )

- production ( azure container apps deployment ) ( azure cli credentials ) ( managed identity ) ( persistent data volumes ) ( medical-grade compliace enabled )

## infrastructure

- podman containers instead of docker ( use aspire instead of compose )

- postgre
- redis
- rabbitmq
- azure key vault
- azure cdn
- blob store
- microsoft entra external id authentication

- prometheus/grafana

### database migrations

- automated development and testing migrations
- manual production migrations

## api gateways

- yarp reverse proxy
- dotnet core rate limiting
- security headers
- cors policies

- handle cross cutting concerns

### public apis gateway

- anonymous usage
- ip-based rate limiting ( 1000 req/min )
- public website origins
- redis backing store
- standard observability
- standard security

### admin apis gateway

- role-based access control
- user-based rate limiting ( 100 req/min )
- medical-grade audit loggging
- medical-grade security

## api domains

- domain shared kernels for public and admin apis ( entities , value objects , specifications business rules , repositories interfaces )
- vertical slice public and admin apis ( repositories , use cases with cqrs , handlers , REST endpoints )

## api versioning

-

## observability

- serilog for logging ( use ILogger instead of serilog directly ) ( use structured logging , not concatenation ) ( each log should have key bits of information ( user ID , Correrlation ID , Request URL , APP Version , and so forth ) ( logs should be developer focused ) ( log levels : debug , information , waarning , error , critical ) ( not having 100% log delivery is okay )

- audits ( for medical-grade compliance ) ( losing any data is unacceptable ) ( store data in the same database as the data that's audited )

## security

- security ( we must have fallback policies that get evaluated if no other policy is specifieid )

# testing

- arrange , act , assert
- contract-first testing ( testing interfaces/contracts rather than implementation details ) ( focused on preconditions/dependencies and postconditions/state-change )
- properly-based testing

- unit tests must use mock for dependencies to craete isolation
- integration tests must use real dependencies ( not mocks ) ( use DistributedApplicationTestingBuilder )
- end to end tests must use real dependencies and be done in aspire ( they should test the website for proper backend to frontend integration ) ( with xunit instead of vitest ) ( use DistributedApplicationTestingBuilder )

- all tests must have timeouts ( they should fail fast if something is wrong ) ( 5 seconds for unit tests ) ( 15 secnds for integration ) ( 30 seconds for end to end tests )

- do not use curl commmands or cli tools for testing ( test through our testing framework )

# version control , continuous integration , continuous delivery

- trunk based development for version control


[ important general rules to follow ]

- remember that this is nixos and the admin password is 'unsecure'
- ultrathink and deep dive and be comprehensive and be professional

- be causious of technical debt
- be causious of overengineering

- do not create documentation
- do not preserve legacy implementations
- do not implement experimental architectures not part of industry
- do not change the UI of our website unless explicitly asked to
- do not create simple ephimeral validation implementations in /temp/ directories to avoid disorder in source files
- do not creaate script files for projects ( this is an anti-pattern )

# Task Management Context Guidelines

- task descriptions must include WHY ( business reason , compliance requirement , architectural decision , so forth )
- task descriptions must include SCOPE ( which APIs , which components , which environments , so forth )
- task descriptions must include DEPENDENCIES ( what must complete first , integration points , so forth )
- task descriptions must include CONTEXT ( gateway architecture , medical compliance , environment specifics , so forth )

- critical : ensure you add proper context to your task list items , in the event context compression happens in the middle of a task , so you have a better idea of what you were working ( your task list is your primary source of context between context compressions , so it needs to be we managed )
- show the tasks list after completing a task
- ensure you update your context before working on the next task


( continue working )
