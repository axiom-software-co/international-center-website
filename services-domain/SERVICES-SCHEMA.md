# Services Domain Database Schema

## Core Tables

### Services Table
```sql
CREATE TABLE services (
    service_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    title VARCHAR(255) NOT NULL,
    short_description TEXT NOT NULL,
    slug VARCHAR(255) UNIQUE NOT NULL,
    long_description_url VARCHAR(500), -- URL to Azure Blob Storage content
    category_id UUID NOT NULL REFERENCES service_categories(category_id),
    image_url VARCHAR(500),
    order_number INTEGER NOT NULL DEFAULT 0,
    delivery_mode VARCHAR(50) NOT NULL CHECK (delivery_mode IN ('mobile_service', 'outpatient_service', 'inpatient_service')),
    publishing_status VARCHAR(20) NOT NULL DEFAULT 'draft' CHECK (publishing_status IN ('draft', 'published', 'archived')),
    
    -- Audit fields
    created_on TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_by VARCHAR(255),
    modified_on TIMESTAMPTZ,
    modified_by VARCHAR(255),
    
    -- Soft delete fields
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE,
    deleted_on TIMESTAMPTZ,
    deleted_by VARCHAR(255)
);
```

### Service Categories Table
```sql
CREATE TABLE service_categories (
    category_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(255) NOT NULL,
    slug VARCHAR(255) UNIQUE NOT NULL,
    order_number INTEGER NOT NULL DEFAULT 0,
    is_default_unassigned BOOLEAN NOT NULL DEFAULT FALSE,
    
    -- Audit fields
    created_on TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_by VARCHAR(255),
    modified_on TIMESTAMPTZ,
    modified_by VARCHAR(255),
    
    -- Soft delete fields  
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE,
    deleted_on TIMESTAMPTZ,
    deleted_by VARCHAR(255),
    
    CONSTRAINT only_one_default_unassigned CHECK (
        NOT is_default_unassigned OR 
        (SELECT COUNT(*) FROM service_categories WHERE is_default_unassigned = TRUE AND is_deleted = FALSE) <= 1
    )
);
```

### Featured Categories Table
```sql
CREATE TABLE featured_categories (
    featured_category_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    category_id UUID NOT NULL REFERENCES service_categories(category_id),
    feature_position INTEGER NOT NULL CHECK (feature_position IN (1, 2)),
    
    -- Audit fields
    created_on TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_by VARCHAR(255),
    modified_on TIMESTAMPTZ,
    modified_by VARCHAR(255),
    
    UNIQUE(feature_position),
    CONSTRAINT no_default_unassigned_featured CHECK (
        NOT EXISTS (
            SELECT 1 FROM service_categories sc 
            WHERE sc.category_id = featured_categories.category_id 
            AND sc.is_default_unassigned = TRUE
        )
    )
);
```

## Medical-Grade Audit Tables

### Services Audit Table
```sql
CREATE TABLE services_audit (
    audit_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    service_id UUID NOT NULL,
    operation_type VARCHAR(20) NOT NULL CHECK (operation_type IN ('INSERT', 'UPDATE', 'DELETE')),
    audit_timestamp TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    user_id VARCHAR(255),
    correlation_id UUID,
    
    -- Snapshot of data at time of operation
    title VARCHAR(255),
    short_description TEXT,
    slug VARCHAR(255),
    long_description_url VARCHAR(500), -- URL to Azure Blob Storage content
    category_id UUID,
    image_url VARCHAR(500),
    order_number INTEGER,
    delivery_mode VARCHAR(50),
    publishing_status VARCHAR(20),
    
    -- Audit fields snapshot
    created_on TIMESTAMPTZ,
    created_by VARCHAR(255),
    modified_on TIMESTAMPTZ,
    modified_by VARCHAR(255),
    
    -- Soft delete fields snapshot
    is_deleted BOOLEAN,
    deleted_on TIMESTAMPTZ,
    deleted_by VARCHAR(255)
);
```

### Service Categories Audit Table
```sql
CREATE TABLE service_categories_audit (
    audit_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    category_id UUID NOT NULL,
    operation_type VARCHAR(20) NOT NULL CHECK (operation_type IN ('INSERT', 'UPDATE', 'DELETE')),
    audit_timestamp TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    user_id VARCHAR(255),
    correlation_id UUID,
    
    -- Snapshot of data at time of operation
    name VARCHAR(255),
    slug VARCHAR(255),
    order_number INTEGER,
    is_default_unassigned BOOLEAN,
    
    -- Audit fields snapshot
    created_on TIMESTAMPTZ,
    created_by VARCHAR(255),
    modified_on TIMESTAMPTZ,
    modified_by VARCHAR(255),
    
    -- Soft delete fields snapshot
    is_deleted BOOLEAN,
    deleted_on TIMESTAMPTZ,
    deleted_by VARCHAR(255)
);
```

### Featured Categories Audit Table
```sql
CREATE TABLE featured_categories_audit (
    audit_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    featured_category_id UUID NOT NULL,
    operation_type VARCHAR(20) NOT NULL CHECK (operation_type IN ('INSERT', 'UPDATE', 'DELETE')),
    audit_timestamp TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    user_id VARCHAR(255),
    correlation_id UUID,
    
    -- Snapshot of data at time of operation
    category_id UUID,
    feature_position INTEGER,
    
    -- Audit fields snapshot
    created_on TIMESTAMPTZ,
    created_by VARCHAR(255),
    modified_on TIMESTAMPTZ,
    modified_by VARCHAR(255)
);
```

## Performance Indexes

### Core Table Indexes
```sql
-- Services table indexes
CREATE INDEX idx_services_category_id ON services(category_id) WHERE is_deleted = FALSE;
CREATE INDEX idx_services_publishing_status ON services(publishing_status) WHERE is_deleted = FALSE;
CREATE INDEX idx_services_slug ON services(slug) WHERE is_deleted = FALSE;
CREATE INDEX idx_services_order_category ON services(category_id, order_number) WHERE is_deleted = FALSE;
CREATE INDEX idx_services_delivery_mode ON services(delivery_mode) WHERE is_deleted = FALSE;

-- Service categories table indexes  
CREATE INDEX idx_service_categories_slug ON service_categories(slug) WHERE is_deleted = FALSE;
CREATE INDEX idx_service_categories_order ON service_categories(order_number) WHERE is_deleted = FALSE;
CREATE INDEX idx_service_categories_default ON service_categories(is_default_unassigned) WHERE is_deleted = FALSE;

-- Featured categories table indexes
CREATE INDEX idx_featured_categories_category_id ON featured_categories(category_id);
CREATE INDEX idx_featured_categories_position ON featured_categories(feature_position);
```

### Audit Table Indexes
```sql
-- Services audit indexes
CREATE INDEX idx_services_audit_service_id ON services_audit(service_id);
CREATE INDEX idx_services_audit_timestamp ON services_audit(audit_timestamp);
CREATE INDEX idx_services_audit_operation ON services_audit(operation_type);
CREATE INDEX idx_services_audit_user ON services_audit(user_id);
CREATE INDEX idx_services_audit_correlation ON services_audit(correlation_id);

-- Service categories audit indexes  
CREATE INDEX idx_categories_audit_category_id ON service_categories_audit(category_id);
CREATE INDEX idx_categories_audit_timestamp ON service_categories_audit(audit_timestamp);
CREATE INDEX idx_categories_audit_operation ON service_categories_audit(operation_type);
CREATE INDEX idx_categories_audit_user ON service_categories_audit(user_id);
CREATE INDEX idx_categories_audit_correlation ON service_categories_audit(correlation_id);

-- Featured categories audit indexes
CREATE INDEX idx_featured_audit_featured_id ON featured_categories_audit(featured_category_id);
CREATE INDEX idx_featured_audit_timestamp ON featured_categories_audit(audit_timestamp);
CREATE INDEX idx_featured_audit_operation ON featured_categories_audit(operation_type);
CREATE INDEX idx_featured_audit_user ON featured_categories_audit(user_id);
CREATE INDEX idx_featured_audit_correlation ON featured_categories_audit(correlation_id);
```

## Content Storage Strategy

### Rich Content Management
Services rich content (detailed descriptions, articles) is stored in Azure Blob Storage for optimal performance and cost efficiency.

**Storage Pattern:**
- **Database**: Stores `long_description_url` pointing to blob storage content
- **Blob Storage**: Stores HTML/Markdown files with rich content, images, and media
- **CDN**: Azure CDN delivers content globally with caching

**URL Structure:**
```
https://cdn.internationalcenter.com/services/content/{service-id}/{content-hash}.html
```

**Content Versioning:**
- Content files use hash-based naming for immutability
- URL changes in database trigger audit records
- Previous content versions retained for audit compliance

**Performance Benefits:**
- Smaller database rows = faster queries
- CDN caching reduces origin load
- Blob storage optimized for large content delivery
- Cost-effective storage for rich media content

**Medical-Grade Compliance:**
- Content URL changes audited in `services_audit` table
- Content integrity verified via hash-based storage
- Immutable content versions for regulatory requirements
- Full audit trail of content authoring and approval workflow

## Database Functions and Triggers

### Auto-Audit Trigger Function
```sql
CREATE OR REPLACE FUNCTION create_audit_record()
RETURNS TRIGGER AS $$
BEGIN
    -- Implementation will be added during TDD cycle
    RETURN COALESCE(NEW, OLD);
END;
$$ LANGUAGE plpgsql;
```

### Default Category Assignment Function
```sql
CREATE OR REPLACE FUNCTION reassign_services_to_default_category()
RETURNS TRIGGER AS $$
BEGIN
    -- Implementation will be added during TDD cycle
    RETURN OLD;
END;
$$ LANGUAGE plpgsql;
```

## Business Rules

### Category Management Rules
1. **Default Unassigned Category**: Exactly one category must have `is_default_unassigned = TRUE`
2. **Featured Categories**: Exactly two categories can be featured (positions 1 and 2)
3. **Featured Category Restriction**: The default unassigned category cannot be featured
4. **Service Assignment**: All services must have a valid category_id
5. **Category Deletion**: When a category is soft-deleted, all its services are reassigned to the default unassigned category

### Service Publishing Rules
1. **Publishing Status**: Services can be 'draft', 'published', or 'archived'
2. **Default Status**: New services start with 'draft' status
3. **Category Assignment**: All services start with the default unassigned category
4. **Delivery Modes**: Services must specify delivery mode (mobile_service, outpatient_service, inpatient_service)
5. **Content Storage**: Rich content stored in Azure Blob Storage, referenced via `long_description_url`
6. **Content Versioning**: Content URLs use hash-based naming for immutable versions
7. **Content Publishing**: Services can be published without rich content (URL can be NULL)
8. **URL Validation**: All `long_description_url` values must be valid HTTPS URLs when not NULL

### Audit Requirements
1. **Medical-Grade Compliance**: All changes must be audited
2. **Immutable Audit Records**: Audit records cannot be modified or deleted
3. **Complete Data Snapshots**: Audit tables store complete snapshots of data at time of operation
4. **Correlation Tracking**: All related operations share the same correlation_id
5. **User Attribution**: All operations must record the user_id who performed the action

### Data Integrity Rules
1. **Soft Delete Only**: Records are never physically deleted, only marked as deleted
2. **Unique Constraints**: Slugs must be unique across active (non-deleted) records
3. **Order Numbers**: Used for consistent ordering within categories and featured positions
4. **Foreign Key Integrity**: All category_id references must be valid and active
5. **Content URL Integrity**: `long_description_url` must point to existing blob storage content when not NULL
6. **Content Immutability**: Blob storage content files are never modified, only new versions created
7. **URL Format Validation**: Content URLs must follow the standard pattern with service-id and content-hash
8. **Content Lifecycle**: Orphaned content files cleaned up after audit retention period expires