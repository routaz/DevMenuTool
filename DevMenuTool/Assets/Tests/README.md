# DevMenu Tool Unit Tests

This directory contains comprehensive unit tests for the DevMenu tool, following clean code and SOLID principles.

## Test Structure

### Core Tests
- **MethodCacheTests.cs** - Tests for the method caching functionality
- **DevMethodFactoryTests.cs** - Tests for the factory pattern implementation
- **DevMenuContainerTests.cs** - Tests for the dependency injection container
- **DevMenuMethodIndexTests.cs** - Tests for the `[DevMenu]` method index (Set, assembly filtering, background build)
- **MethodScannerIndexTests.cs** - Tests for the index-driven scene scan (hidden/inactive objects, subclasses, RebuildIndex)

### Runtime Tests
- **ParameterHandlerTests.cs** - Tests for individual parameter handlers (String, Numeric, Boolean, Enum)
- **ParameterHandlerRegistryTests.cs** - Tests for the parameter handler registry
- **MethodInvokerTests.cs** - Tests for method invocation logic

### Integration Tests
- **DevMenuIntegrationTests.cs** - End-to-end tests that verify the complete workflow

### Test Helpers
- **MockDevMethod.cs** - Reusable mock implementation for testing

## Running Tests

### In Unity Editor
1. Open the Test Runner window: `Window > General > Test Runner`
2. Select the "DevMenuTool.Tests" assembly
3. Click "Run All" or run individual test classes

### Via Command Line
```bash
# Run all tests
Unity.exe -batchmode -quit -projectPath <project-path> -runTests -testPlatform EditMode -testResults <results-file>

# Run specific test assembly
Unity.exe -batchmode -quit -projectPath <project-path> -runTests -testPlatform EditMode -testFilter "DevMenuTool.Tests"
```

## Test Categories

### Unit Tests
- **MethodCacheTests**: Tests caching behavior, duplicate prevention, and cache clearing
- **ParameterHandlerTests**: Tests individual parameter type handling
- **DevMethodFactoryTests**: Tests factory pattern and object creation
- **DevMenuContainerTests**: Tests dependency injection and singleton behavior

### Integration Tests
- **DevMenuIntegrationTests**: Tests complete workflows including:
  - Method discovery and scanning
  - Parameter handling with different types
  - Method invocation with various parameter combinations
  - Multiple object scenarios

## Adding New Tests

### For New Parameter Types
1. Create a new handler in `Runtime/Handlers/`
2. Add it to `ParameterHandlerRegistry`
3. Create tests in `ParameterHandlerTests.cs`
4. Add integration tests in `DevMenuIntegrationTests.cs`

### For New Features
1. Create unit tests for the new functionality
2. Add integration tests to verify end-to-end behavior
3. Update existing tests if interfaces change

## Test Best Practices

### Naming Convention
- Test methods: `MethodName_Scenario_ExpectedResult`
- Test classes: `ClassNameTests`
- Mock classes: `MockClassName`

### Test Structure (AAA Pattern)
```csharp
[Test]
public void MethodName_WithValidInput_ShouldReturnExpectedResult()
{
    // Arrange
    var input = "test";
    
    // Act
    var result = methodUnderTest.Process(input);
    
    // Assert
    Assert.That(result, Is.EqualTo("expected"));
}
```

### Mocking Strategy
- Use `MockDevMethod` for testing with IDevMethod dependencies
- Create specific mocks for complex scenarios
- Prefer constructor injection for testability

### Test Isolation
- Each test should be independent
- Use `[SetUp]` and `[TearDown]` for common initialization
- Reset static state in `[TearDown]` methods

## Coverage Areas

### Core Components
- ✅ Method caching and retrieval
- ✅ Factory pattern implementation
- ✅ Dependency injection container
- ✅ Interface segregation

### Runtime Components
- ✅ Parameter type handling (String, Int, Float, Bool, Enum)
- ✅ Parameter registry and strategy pattern
- ✅ Method invocation with parameter resolution
- ✅ Error handling and logging

### Integration Scenarios
- ✅ Complete method discovery workflow
- ✅ Parameter handling with defaults
- ✅ Multiple object scenarios
- ✅ Enum parameter handling

## Future Test Additions

### UI Testing
- Test UIBuilder component creation
- Test UI event handling
- Test parameter field generation

### Editor Testing
- Test editor window functionality
- Test parameter persistence
- Test key binding management

### Performance Testing
- Test method scanning performance
- Test parameter parsing performance
- Test memory usage patterns

## Troubleshooting

### Common Issues
1. **Tests not running**: Ensure the test assembly is properly referenced
2. **Mock failures**: Check that mock objects implement interfaces correctly
3. **Static state issues**: Use `DevMenuContainer.Reset()` in `[TearDown]`

### Debugging Tests
- Use `Debug.Log()` in test methods for debugging
- Check Unity Console for test output
- Use breakpoints in test methods

## Continuous Integration

### GitHub Actions Example
```yaml
name: Run Tests
on: [push, pull_request]
jobs:
  test:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v2
    - name: Run Unity Tests
      run: |
        Unity.exe -batchmode -quit -projectPath . -runTests -testPlatform EditMode -testResults TestResults.xml
```

This testing structure ensures that the DevMenu tool is robust, maintainable, and follows clean code principles while providing comprehensive coverage of all functionality. 