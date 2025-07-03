namespace Domain.Exceptions;

public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message) { }
    protected DomainException(string message, Exception innerException) : base(message, innerException) { }
}

public class ConfigurationNotFoundException : DomainException
{
    public ConfigurationNotFoundException(Guid id) 
        : base($"Configuration with ID '{id}' was not found.") { }
    
    public ConfigurationNotFoundException(string key, string environment) 
        : base($"Configuration with key '{key}' in environment '{environment}' was not found.") { }
}

public class ConfigurationAlreadyExistsException : DomainException
{
    public ConfigurationAlreadyExistsException(string key, string environment) 
        : base($"Configuration with key '{key}' already exists in environment '{environment}'.") { }
}

public class EnvironmentNotFoundException : DomainException
{
    public EnvironmentNotFoundException(Guid id) 
        : base($"Environment with ID '{id}' was not found.") { }
    
    public EnvironmentNotFoundException(string name) 
        : base($"Environment with name '{name}' was not found.") { }
}

public class EnvironmentAlreadyExistsException : DomainException
{
    public EnvironmentAlreadyExistsException(string name) 
        : base($"Environment with name '{name}' already exists.") { }
}

public class ConfigurationGroupNotFoundException : DomainException
{
    public ConfigurationGroupNotFoundException(Guid id) 
        : base($"Configuration group with ID '{id}' was not found.") { }
    
    public ConfigurationGroupNotFoundException(string name) 
        : base($"Configuration group with name '{name}' was not found.") { }
}

public class ConfigurationGroupAlreadyExistsException : DomainException
{
    public ConfigurationGroupAlreadyExistsException(string name) 
        : base($"Configuration group with name '{name}' already exists.") { }
}

public class InvalidConfigurationValueException : DomainException
{
    public InvalidConfigurationValueException(string value, string type) 
        : base($"Value '{value}' is not valid for type '{type}'.") { }
}

public class CircularReferenceException : DomainException
{
    public CircularReferenceException(string groupName) 
        : base($"Circular reference detected in configuration group hierarchy for group '{groupName}'.") { }
}
