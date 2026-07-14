# Security Analysis Report - PharmaCore Backend

**Date**: 2024  
**Analyst**: AI Security Analyst  
**Scope**: Full backend security assessment  

---

## Executive Summary

This report presents the findings of a comprehensive security analysis of the PharmaCore backend system. The analysis identified several security weaknesses across authentication, authorization, data protection, and configuration management. The most critical issues include missing role-based authorization on sensitive endpoints, weak JWT secret key, and lack of rate limiting on authentication endpoints.

---

## Critical Severity Findings

### 1. Missing Role-Based Authorization on Sensitive Endpoints

**Location**: `PharmaCore.API/Controllers/UsersController.cs`  
**Issue**: The `UsersController` has `[Authorize]` at the class level, but there's no role-based authorization on sensitive endpoints like `Create`, `Update`, `Delete`, and `HardDelete`.

**Impact**: Any authenticated user (including Cashiers) can create, modify, or delete users, including other Admins. This is a privilege escalation vulnerability that could lead to unauthorized access and data manipulation.

**Recommendation**: Add `[Authorize(Roles = "ADMIN")]` to the following methods:
- `Create` (POST `/users`)
- `Update` (PUT `/users/{id}`)
- `Delete` (DELETE `/users/{id}`)
- `HardDelete` (DELETE `/users/{id}/hard`)
- `Restore` (POST `/users/{id}/restore`)
- `ListDeleted` (GET `/users/deleted`)

### 2. Weak JWT Secret Key

**Location**: `.env` file and `appsettings.json`  
**Issue**: The JWT secret key is a placeholder `"your-super-long-secure-development-key-2026-min-32-chars"` which is not cryptographically secure.

**Impact**: Attackers can forge JWT tokens and impersonate any user, gaining unauthorized access to the system.

**Recommendation**: 
- Generate a cryptographically secure random secret key (at least 64 characters)
- Store it securely using environment variables or a secrets manager
- Never commit the actual secret key to version control

### 3. Hardcoded Database Credentials with Weak Password

**Location**: `.env` file and `appsettings.json`  
**Issue**: Database credentials are hardcoded with a weak password (`123`).

**Impact**: If the repository is compromised, database access is immediately available to attackers, potentially leading to data breaches and data manipulation.

**Recommendation**:
- Use environment variables or a secrets manager for database credentials
- Enforce strong password policies for database users
- Consider using certificate-based authentication for PostgreSQL

---

## High Severity Findings

### 4. No Rate Limiting on Authentication Endpoints

**Location**: `PharmaCore.API/Program.cs`  
**Issue**: There's no rate limiting configured on the login endpoint or other sensitive endpoints.

**Impact**: Attackers can perform brute force attacks on user credentials without any restrictions, potentially compromising user accounts.

**Recommendation**: Implement rate limiting middleware, especially on the `/auth/login` endpoint. Consider using a sliding window or fixed window approach with appropriate limits (e.g., 5 attempts per minute per IP).

### 5. In-Memory Token Revocation

**Location**: `PharmaCore.Infrastructure/Security/InMemoryTokenRevocationService.cs`  
**Issue**: Token revocation uses an in-memory `ConcurrentDictionary`, which means revoked tokens are lost on application restart.

**Impact**: Revoked tokens can be reused after application restart, compromising logout functionality and allowing unauthorized access.

**Recommendation**: 
- Use a persistent storage mechanism (Redis, database) for token revocation
- Consider implementing a token blacklist with expiration times
- Ensure the revocation mechanism is scalable and fault-tolerant

### 6. Backup Path Exposure

**Location**: `PharmaCore.Infrastructure/System/Services/BackupDatabaseService.cs`  
**Issue**: The backup service returns the full file path in the response (`backupPath`).

**Impact**: This exposes server directory structure to potential attackers, aiding in reconnaissance and potential attacks.

**Recommendation**: Return only the backup filename, not the full path. If the path is necessary for download, implement proper access controls and consider using temporary signed URLs.

---

## Medium Severity Findings

### 7. Missing Security Headers

**Location**: `PharmaCore.API/Program.cs`  
**Issue**: No security headers are configured (HSTS, X-Content-Type-Options, X-Frame-Options, etc.).

**Impact**: The application is vulnerable to clickjacking, MIME-type sniffing attacks, and other browser-based attacks.

**Recommendation**: Add security headers middleware with the following headers:
- `Strict-Transport-Security` (HSTS)
- `X-Content-Type-Options: nosniff`
- `X-Frame-Options: DENY`
- `X-XSS-Protection: 1; mode=block`
- `Referrer-Policy: strict-origin-when-cross-origin`

### 8. Weak Password Validation

**Location**: `PharmaCore.Application/Users/Services/CreateUserService.cs`  
**Issue**: Password validation only checks minimum length (6 characters) without complexity requirements.

**Impact**: Users can set weak passwords that are easily guessable, increasing the risk of credential theft.

**Recommendation**: Enforce password complexity requirements:
- Minimum 8 characters
- At least one uppercase letter
- At least one lowercase letter
- At least one number
- At least one special character

### 9. Swagger/OpenAPI in Development

**Location**: `PharmaCore.API/Program.cs`  
**Issue**: Swagger UI is exposed in the development environment.

**Impact**: If the development environment is accessible, API structure is exposed to potential attackers, providing valuable information for reconnaissance.

**Recommendation**: 
- Ensure Swagger is only available in development and not in production
- Consider using environment-specific configuration to disable Swagger in production
- Add authentication to Swagger UI if it must be accessible in non-production environments

---

## Low Severity Findings

### 10. Missing Input Validation on Some Endpoints

**Location**: Various controllers  
**Issue**: Some endpoints lack proper input validation for query parameters and request bodies.

**Impact**: Potential for injection attacks or unexpected behavior.

**Recommendation**: 
- Add comprehensive input validation using FluentValidation or Data Annotations
- Validate all input parameters, including query strings, route parameters, and request bodies
- Implement sanitization for user inputs

### 11. CORS Configuration

**Location**: `PharmaCore.API/Program.cs`  
**Issue**: CORS policy allows specific localhost and IP addresses, which might be too permissive.

**Impact**: Potential for cross-origin attacks if the configuration is not properly maintained.

**Recommendation**: 
- Review and restrict CORS origins to only necessary domains
- Use environment-specific CORS configurations
- Avoid using wildcard origins in production

---

## Positive Findings

### 1. Password Hashing
The application uses PBKDF2 with SHA256 and 100,000 iterations, which is a secure hashing algorithm. This provides good protection against brute force attacks on password hashes.

### 2. JWT Validation
The JWT configuration validates issuer, audience, signing key, and lifetime. This ensures that tokens are properly validated and prevents token forgery.

### 3. Token Revocation
The application implements token revocation for logout functionality, which is important for security. However, the implementation should be made persistent (see Finding #5).

### 4. Soft Delete
The application uses soft delete for users, which is a good practice for data recovery and audit trails.

### 5. Database Backup/Restore
The application has backup and restore functionality with proper authorization (ADMIN role for restore). This ensures that only authorized users can perform critical operations.

---

## Compliance Mapping

### MITRE ATT&CK
- **T1078 (Valid Accounts)**: Findings #1, #2, #4 relate to unauthorized access through valid accounts
- **T1110 (Brute Force)**: Finding #4 relates to brute force attacks on authentication
- **T1552 (Credentials in Files)**: Findings #2, #3 relate to credentials stored insecurely

### NIST CSF 2.0
- **PR.AC (Identity Management and Access Control)**: Findings #1, #2, #4, #8
- **PR.DS (Data Security)**: Findings #2, #3, #6
- **PR.PT (Protective Technology)**: Findings #7, #9, #11

### OWASP Top 10 2021
- **A01:2021 (Broken Access Control)**: Finding #1
- **A02:2021 (Cryptographic Failures)**: Findings #2, #3
- **A07:2021 (Identification and Authentication Failures)**: Findings #4, #8

---

## Recommendations Summary

### Immediate Actions (Critical/High Severity)
1. Add role-based authorization to user management endpoints
2. Generate and securely store a cryptographically secure JWT secret key
3. Implement rate limiting on authentication endpoints
4. Remove hardcoded database credentials and use secure credential management
5. Implement persistent token revocation (Redis or database)

### Short-term Actions (Medium Severity)
1. Add security headers middleware
2. Enhance password validation with complexity requirements
3. Fix backup path exposure
4. Restrict Swagger access in non-development environments

### Long-term Actions (Low Severity)
1. Implement comprehensive input validation
2. Regular security audits and penetration testing
3. Implement logging and monitoring for security events
4. Consider implementing API versioning and deprecation policies
5. Implement automated security scanning in CI/CD pipeline

---

## Conclusion

The PharmaCore backend has several security weaknesses that need to be addressed, particularly in the areas of authorization, authentication, and data protection. The critical and high severity issues should be addressed immediately to prevent potential security breaches. The application does have some positive security features, such as secure password hashing and JWT validation, which provide a good foundation for building a more secure system.

By implementing the recommended fixes and following the suggested best practices, the security posture of the PharmaCore backend can be significantly improved. Regular security assessments and monitoring should be implemented to maintain and enhance the security of the system over time.

---

**Report Generated**: 2024  
**Classification**: Confidential  
**Distribution**: Security Team, Development Team
