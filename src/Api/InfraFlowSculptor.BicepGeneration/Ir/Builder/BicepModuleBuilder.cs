namespace InfraFlowSculptor.BicepGeneration.Ir.Builder;

/// <summary>
/// Fluent builder for constructing <see cref="BicepModuleSpec"/> instances.
/// Used by migrated generators to replace legacy <c>const string</c> templates.
/// </summary>
public sealed class BicepModuleBuilder
{
    private string? _moduleName;
    private string? _folderName;
    private string? _resourceTypeName;
    private string? _resourceSymbol;
    private string? _armType;
    private string? _parentSymbol;
    private readonly List<BicepImport> _imports = [];
    private readonly List<BicepParam> _parameters = [];
    private readonly List<BicepVar> _variables = [];
    private readonly List<BicepPropertyAssignment> _resourceBody = [];
    private readonly List<BicepOutput> _outputs = [];
    private readonly List<BicepTypeDefinition> _exportedTypes = [];
    private readonly List<BicepCompanionSpec> _companions = [];
    private readonly List<BicepExistingResource> _existingResources = [];

    /// <summary>Sets the module identity (name, folder, resource type name).</summary>
    public BicepModuleBuilder Module(string name, string folder, string resourceTypeName)
    {
        _moduleName = name;
        _folderName = folder;
        _resourceTypeName = resourceTypeName;
        return this;
    }

    /// <summary>Sets the primary resource declaration identity (symbol and ARM type with API version).</summary>
    public BicepModuleBuilder Resource(string symbol, string armTypeWithApiVersion)
    {
        _resourceSymbol = symbol;
        _armType = armTypeWithApiVersion;
        return this;
    }

    /// <summary>Adds an import statement.</summary>
    public BicepModuleBuilder Import(string path, params string[] symbols)
    {
        _imports.Add(new BicepImport(path, symbols.Length > 0 ? symbols.ToList() : null));
        return this;
    }

    /// <summary>Adds a parameter declaration.</summary>
    public BicepModuleBuilder Param(string name, BicepType type, string? description = null,
        bool secure = false, BicepExpression? defaultValue = null)
    {
        _parameters.Add(new BicepParam(name, type, description, secure, defaultValue));
        return this;
    }

    /// <summary>Adds a variable declaration.</summary>
    public BicepModuleBuilder Var(string name, BicepExpression expression)
    {
        _variables.Add(new BicepVar(name, expression));
        return this;
    }

    /// <summary>Adds an existing resource reference (e.g. for parent lookups).</summary>
    public BicepModuleBuilder ExistingResource(string symbol, string armTypeWithApiVersion, string nameExpression)
    {
        _existingResources.Add(new BicepExistingResource(symbol, armTypeWithApiVersion, nameExpression));
        return this;
    }

    /// <summary>Sets the parent symbol for the primary resource (emits <c>parent: symbol</c>).</summary>
    public BicepModuleBuilder Parent(string parentSymbol)
    {
        _parentSymbol = parentSymbol;
        return this;
    }

    /// <summary>Adds a top-level property to the resource body.</summary>
    public BicepModuleBuilder Property(string key, BicepExpression value)
    {
        _resourceBody.Add(new BicepPropertyAssignment(key, value));
        return this;
    }

    /// <summary>Adds a top-level property with a nested object value built by the given action.</summary>
    public BicepModuleBuilder Property(string key, Action<BicepObjectBuilder> nestedBuilder)
    {
        var nested = new BicepObjectBuilder();
        nestedBuilder(nested);
        _resourceBody.Add(new BicepPropertyAssignment(key, nested.Build()));
        return this;
    }

    /// <summary>Adds an output declaration.</summary>
    public BicepModuleBuilder Output(string name, BicepType type, BicepExpression expression,
        bool secure = false, string? description = null)
    {
        _outputs.Add(new BicepOutput(name, type, expression, secure, description));
        return this;
    }

    /// <summary>Adds a type definition for <c>types.bicep</c>.</summary>
    public BicepModuleBuilder ExportedType(string name, BicepExpression body, string? description = null)
    {
        _exportedTypes.Add(new BicepTypeDefinition(name, body, IsExported: true, description));
        return this;
    }

    /// <summary>Adds a companion module specification.</summary>
    public BicepModuleBuilder Companion(string moduleName, string folderName, BicepModuleSpec spec)
    {
        _companions.Add(new BicepCompanionSpec(moduleName, folderName, spec));
        return this;
    }

    /// <summary>
    /// Builds the immutable <see cref="BicepModuleSpec"/>.
    /// Throws <see cref="InvalidOperationException"/> when required properties are missing.
    /// </summary>
    public BicepModuleSpec Build()
    {
        if (_moduleName is null || _folderName is null || _resourceTypeName is null)
            throw new InvalidOperationException("Module identity (name, folder, resourceTypeName) must be set via Module().");

        if (_resourceSymbol is null || _armType is null)
            throw new InvalidOperationException("Resource identity (symbol, armType) must be set via Resource().");

        return new BicepModuleSpec
        {
            ModuleName = _moduleName,
            ModuleFolderName = _folderName,
            ResourceTypeName = _resourceTypeName,
            Imports = _imports.ToList(),
            Parameters = _parameters.ToList(),
            Variables = _variables.ToList(),
            ExistingResources = _existingResources.ToList(),
            Resource = new BicepResourceDeclaration
            {
                Symbol = _resourceSymbol,
                ArmTypeWithApiVersion = _armType,
                ParentSymbol = _parentSymbol,
                Body = _resourceBody.ToList(),
            },
            Outputs = _outputs.ToList(),
            ExportedTypes = _exportedTypes.ToList(),
            Companions = _companions.ToList(),
        };
    }
}
