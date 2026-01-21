# Changelog

All notable changes to the Posty5 .NET SDK will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2026-01-21

### Added

- Initial release of Posty5 .NET SDK
- Posty5.Core package with HTTP client and base classes
- Posty5.QRCode package for QR code management
  - Support for URL, Email, WiFi, Call, SMS, FreeText, and Geolocation QR codes
  - CRUD operations for QR codes
  - List and search functionality
- Posty5.ShortLink package for URL shortening
  - Create custom short links
  - Update and delete short links
  - Track click statistics
- Posty5.HtmlHosting package for HTML page hosting
  - Create and host HTML pages
  - Update hosted content
  - Manage multiple pages
- Posty5.SocialPublisher package for social media management
  - Workspace management
  - Task creation and scheduling
  - Multi-platform publishing support
- Comprehensive error handling with custom exceptions
- Retry logic with Polly for resilient HTTP requests
- Support for .NET 8.0
- XML documentation for all public APIs
- Example code and getting started guide

### Dependencies

- Polly 8.3.0
- Polly.Extensions.Http 3.0.0
- System.Text.Json 8.0.0

## [Unreleased]

### Planned

- Support for file uploads
- Webhook event handling
- Analytics and reporting features
- Async enumerable support for pagination
- Additional QR code customization options
