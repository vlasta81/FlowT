# Security Policy

## Supported versions

| Version | Supported |
|---------|-----------|
| 1.3.x   | ✅ Active  |
| 1.2.x   | ⚠️ Critical fixes only |
| < 1.2   | ❌ No longer supported |

## Reporting a vulnerability

**Do not open a public GitHub Issue for security vulnerabilities.**

Please report security issues privately via one of these channels:

- **GitHub Private Vulnerability Reporting** — use the [Security tab](https://github.com/vlasta81/FlowT/security/advisories/new) on this repository
- **Email** — contact the maintainer directly (see GitHub profile)

### What to include

- Description of the vulnerability and potential impact
- Affected FlowT version(s)
- Steps to reproduce or a minimal proof-of-concept
- Suggested fix if you have one

### Response timeline

| Step | Target |
|------|--------|
| Acknowledgement | Within 48 hours |
| Initial assessment | Within 5 business days |
| Fix or mitigation | Depends on severity |

## Scope

Security issues relevant to FlowT include:

- Unsafe code paths in `FlowContext`, `FlowPipeline`, or plugin resolution
- Dependency injection scope leaks that could expose sensitive services
- Analyzer rules that fail to detect unsafe patterns they claim to catch
- Vulnerabilities in FlowT's NuGet dependencies

## Out of scope

- Vulnerabilities in user-defined handlers, policies, or specifications
- Issues in application code that uses FlowT (not in FlowT itself)
- Third-party benchmark dependencies (DispatchR, MediatR, etc.)
