# AdvancedTodoLearningCards.Tests

Unit test project for the AdvancedTodoLearningCards application.

## Test Framework

- **xUnit** - Testing framework
- **Moq** - Mocking library for dependencies
- **FluentAssertions** - Fluent assertion library for readable tests
- **EF Core InMemory** - In-memory database for integration tests

## Test Structure

```
AdvancedTodoLearningCards.Tests/
├── Services/
│   ├── CardServiceTests.cs       - Tests for CardService
│   └── AdminServiceTests.cs      - Tests for AdminService
└── Models/
    └── CardModelTests.cs          - Tests for Card model validation
```

## Running Tests

### Run all tests
```bash
dotnet test
```

### Run tests with detailed output
```bash
dotnet test --verbosity detailed
```

### Run tests with code coverage
```bash
dotnet test /p:CollectCoverage=true
```

## Test Coverage

### Services
- **CardServiceTests** (8 tests)
  - Create card with image URL
  - Get all cards for user
  - Get card by ID
  - Update card
  - Delete card
  - Search cards
  - Authorization checks

- **AdminServiceTests** (6 tests)
  - Dashboard statistics
  - Cards by difficulty distribution
  - Daily reviews analytics
  - User statistics
  - User search/filtering

### Models
- **CardModelTests** (10 tests)
  - Property initialization
  - Image URL validation
  - Title validation
  - MaxLength validation
  - Difficulty levels
  - Timestamps

## Test Results

```
Test summary: total: 26, failed: 0, succeeded: 26, skipped: 0
```

## Adding New Tests

1. Create a new test class in the appropriate folder
2. Inherit from `IDisposable` if using resources
3. Use `[Fact]` for single tests or `[Theory]` for parameterized tests
4. Follow the Arrange-Act-Assert pattern
5. Use FluentAssertions for readable assertions

### Example Test

```csharp
[Fact]
public async Task MethodName_ShouldDoSomething_WhenCondition()
{
    // Arrange
    var mockRepo = new Mock<IRepository>();
    mockRepo.Setup(r => r.GetAsync(1)).ReturnsAsync(new Entity());
    
    // Act
    var result = await service.GetAsync(1);
    
    // Assert
    result.Should().NotBeNull();
    result.Id.Should().Be(1);
}
```

## Continuous Integration

These tests can be integrated into CI/CD pipelines:

```yaml
# Example GitHub Actions
- name: Run Tests
  run: dotnet test --no-build --verbosity normal
```

## Notes

- All tests use in-memory databases or mocks to avoid external dependencies
- Tests are isolated and can run in parallel
- Each test class disposes of resources properly
- Mock objects verify expected behavior
