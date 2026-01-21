# Contributing to Posty5 .NET SDK

Thank you for your interest in contributing to the Posty5 .NET SDK! This document provides guidelines and instructions for contributing.

## Code of Conduct

By participating in this project, you agree to maintain a respectful and inclusive environment for all contributors.

## How to Contribute

### Reporting Bugs

1. Check if the bug has already been reported in [Issues](https://github.com/posty5/posty5-dotnet-sdk/issues)
2. If not, create a new issue with:
   - Clear title and description
   - Steps to reproduce
   - Expected vs actual behavior
   - .NET version and OS information
   - Code samples if applicable

### Suggesting Features

1. Check existing issues and discussions
2. Create a new issue with the "enhancement" label
3. Describe the feature and its use case
4. Include example code if possible

### Pull Requests

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Make your changes
4. Add or update tests
5. Update documentation
6. Commit your changes (`git commit -m 'Add amazing feature'`)
7. Push to the branch (`git push origin feature/amazing-feature`)
8. Open a Pull Request

## Development Setup

### Prerequisites

- .NET 8.0 SDK or higher
- Visual Studio 2022, VS Code, or Rider
- Git

### Building the Project

```bash
git clone https://github.com/posty5/posty5-dotnet-sdk.git
cd posty5-dotnet-sdk
dotnet restore
dotnet build
```

### Running Tests

```bash
dotnet test
```

### Code Style

- Follow C# coding conventions
- Use meaningful variable and method names
- Add XML documentation comments for public APIs
- Keep methods focused and concise
- Use async/await for asynchronous operations

### Commit Messages

- Use present tense ("Add feature" not "Added feature")
- Use imperative mood ("Move cursor to..." not "Moves cursor to...")
- Limit the first line to 72 characters
- Reference issues and pull requests when applicable

### Example Commit Message

```
Add support for custom QR code colors

- Add ColorOptions class
- Update QRCodeClient to accept color parameters
- Add tests for color customization
- Update documentation

Fixes #123
```

## Project Structure

```
posty5-dotnet-sdk/
├── src/
│   ├── Posty5.Core/          # Core functionality
│   ├── Posty5.QRCode/        # QR code features
│   ├── Posty5.ShortLink/     # Short link features
│   ├── Posty5.HtmlHosting/   # HTML hosting features
│   └── Posty5.SocialPublisher/ # Social publishing features
├── tests/
│   └── Posty5.Tests/         # Unit and integration tests
└── examples/                  # Example code
```

## Testing Guidelines

- Write unit tests for new features
- Ensure all tests pass before submitting PR
- Aim for high code coverage
- Use meaningful test names
- Mock external dependencies

## Documentation

- Update README.md if you change functionality
- Add XML comments to public APIs
- Update CHANGELOG.md
- Include code examples for new features

## Questions?

Feel free to open an issue with the "question" label or email support@posty5.com

## License

By contributing, you agree that your contributions will be licensed under the MIT License.
