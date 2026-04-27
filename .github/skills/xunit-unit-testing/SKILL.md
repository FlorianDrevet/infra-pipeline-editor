# Skill : xunit-unit-testing — Tests unitaires xUnit du projet

> **À charger pour toute création, correction, revue, ou extension de tests unitaires .NET/xUnit dans ce dépôt.**
> Ce skill complète `dotnet-patterns`. Il s'applique aux tests unitaires backend et génération .NET, pas aux tests Angular, E2E, ou aux golden/parity tests.

---

## 1. Où écrire les tests dans ce repo

- **Tous les projets de tests vivent sous `tests/`.**
- **Un projet de tests cible un assembly et un seul.**
- **Nom du projet de tests :** `<TargetAssembly>.Tests`.
- **Chemin attendu :** `tests/<TargetAssembly>.Tests/<TargetAssembly>.Tests.csproj`.
- **Référence projet :** un seul `ProjectReference` vers l'assembly testé.
- **Ne jamais écrire de tests unitaires dans** `tests/InfraFlowSculptor.GenerationParity.Tests`.
  Ce projet reste réservé aux golden/parity tests byte-for-byte de génération.

Exemples attendus dans ce dépôt :

- `src/Api/InfraFlowSculptor.Domain` -> `tests/InfraFlowSculptor.Domain.Tests`
- `src/Api/InfraFlowSculptor.Application` -> `tests/InfraFlowSculptor.Application.Tests`
- `src/Api/InfraFlowSculptor.Infrastructure` -> `tests/InfraFlowSculptor.Infrastructure.Tests`
- `src/Api/InfraFlowSculptor.BicepGeneration` -> `tests/InfraFlowSculptor.BicepGeneration.Tests`

Pour les moteurs de génération :

- Les **règles algorithmiques atomiques** vont dans un projet `.Tests` dédié.
- Les **comparaisons d'artefacts complets** restent dans `InfraFlowSculptor.GenerationParity.Tests`.

---

## 2. Template de projet de tests

Si le projet `.Tests` n'existe pas encore, le créer sous `tests/`, l'ajouter à `InfraFlowSculptor.slnx`, et utiliser les conventions suivantes :

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
    <RootNamespace>InfraFlowSculptor.Domain.Tests</RootNamespace>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" />
    <PackageReference Include="xunit" />
    <PackageReference Include="xunit.runner.visualstudio">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\src\Api\InfraFlowSculptor.Domain\InfraFlowSculptor.Domain.csproj" />
  </ItemGroup>

</Project>
```

Règles associées :

- Conserver `net10.0`, comme le test project déjà présent.
- Utiliser la **gestion centralisée des versions** dans `Directory.Packages.props`.
- Si une librairie de test supplémentaire est nécessaire, **ajouter d'abord sa version dans `Directory.Packages.props`**, puis la référencer sans `Version=` dans le `.csproj`.
- Ne pas précharger des dépendances inutiles dans tous les projets de tests.

---

## 3. Toolbox standard à privilégier

Base minimale :

- `Microsoft.NET.Test.Sdk`
- `xunit`
- `xunit.runner.visualstudio`

Outils à utiliser **quand le scénario le justifie** :

- **Assertions** : `FluentAssertions`
- **Substituts / mocks** : `NSubstitute`
- **Snapshot testing** : `Verify.Xunit`
- **Données réalistes** : `Bogus`
- **IQueryable / DbSet mockés** : `MockQueryable.NSubstitute`
- **Temps contrôlé** : `Microsoft.Extensions.TimeProvider.Testing` avec `FakeTimeProvider`

Règles :

- `FluentAssertions` est l'outil par défaut pour les assertions lisibles.
- `NSubstitute` est l'outil par défaut pour les dépendances substituables.
- `Verify.Xunit` est préférable pour des objets complexes, contrats, payloads, ou graphes sérialisables stables.
- `Bogus` est utile pour enrichir les cas réalistes, mais toujours avec un seed fixe.
- Pour les logs : `NullLogger<T>.Instance` par défaut. N'utiliser un logger de test que si le log fait partie du comportement vérifié.

---

## 4. Règles de conception des tests

- Un test unitaire vérifie **une hypothèse et une seule**.
- On teste le **comportement observable**, jamais l'implémentation interne.
- Le test doit être **déterministe**, **isolé**, **rapide**, **lisible**, et **court**.
- Une correction de bug doit commencer par un **test rouge** si le bug est testable au niveau unitaire.

Conventions de nommage :

- **Classe de test** : `<SutName>Tests`
- **Fichier** : `<SutName>Tests.cs`
- **Méthode de test** : `Given_{Preconditions}_When_{StateUnderTest}_Then_{ExpectedBehavior}`
- Suffixer par `_Async` quand la méthode testée est asynchrone.

Conventions SUT :

- Variable locale : `sut`
- Champ de classe : `_sut`

Le skill `dotnet-patterns` demande des XML docs pour le code de production. **Cette règle ne s'applique pas aux tests xUnit** : le nom du test porte l'intention, pas un commentaire XML.

---

## 5. Structure AAA et lifecycle xUnit

Les tests suivent le pattern **Arrange / Act / Assert**.

```csharp
public sealed class ExampleServiceTests
{
    private readonly IDependency _dependency;
    private readonly ExampleService _sut;

    public ExampleServiceTests()
    {
        _dependency = Substitute.For<IDependency>();
        _sut = new ExampleService(_dependency);
    }

    [Fact]
    public void Given_ValidInput_When_DoWork_Then_ReturnsExpectedValue()
    {
        // Arrange
        const string input = "alpha";
        const string expectedValue = "ALPHA";
        _dependency.Transform(input).Returns(expectedValue);

        // Act
        var result = _sut.DoWork(input);

        // Assert
        result.Should().Be(expectedValue);
    }
}
```

Rappels xUnit modernes :

- xUnit instancie une **nouvelle classe de test par cas de test**.
- Utiliser le **constructeur** pour le setup synchrone partagé.
- Utiliser `IAsyncLifetime` pour le setup/cleanup async.
- Éviter les `CollectionFixture` ou `ClassFixture` pour de l'état mutable partagé.
- Ne jamais dépendre de l'ordre d'exécution des tests.

---

## 6. Fact, Theory, InlineData, MemberData

- Utiliser `[Fact]` pour un cas autonome.
- Utiliser `[Theory]` quand **le même comportement et la même assertion** sont validés pour plusieurs entrées.
- Utiliser `[InlineData]` pour des types simples.
- Utiliser `MemberData` ou `ClassData` pour des objets plus riches ou des jeux de données volumineux.

Règle importante :

- Si les entrées empruntent des branches algorithmiques différentes mais mènent exactement à la même assertion métier, une `Theory` est adaptée.
- Si les attentes divergent, créer des tests distincts.

---

## 7. Isolation et gestion des dépendances

Typologies à utiliser consciemment :

- **Dummy** : juste pour satisfaire une signature.
- **Stub** : retourne des données prédéfinies.
- **Fake** : implémentation simplifiée mais fonctionnelle.
- **Mock/Substitute** : vérifie des interactions, seulement si ces interactions font partie du comportement attendu.

Règles :

- Préférer un vrai objet simple à un mock si le coût cognitif est plus faible.
- Ne substituer que les dépendances nécessaires à l'isolement du SUT.
- Ne jamais appeler base de données, réseau, système de fichiers, horloge système, ou services tiers dans un test unitaire.
- Dans ce projet, privilégier la substitution des interfaces applicatives/infrastructure (`IRepository<T>`, `ICurrentUser`, clients Refit, etc.).

Exemple NSubstitute :

```csharp
public sealed class EmailSenderTests
{
    private readonly IEmailClient _emailClient;
    private readonly EmailSender _sut;

    public EmailSenderTests()
    {
        _emailClient = Substitute.For<IEmailClient>();
        _sut = new EmailSender(_emailClient);
    }

    [Fact]
    public void Given_ValidEmailAddress_When_SendWelcomeEmail_Then_ReturnsTrue()
    {
        // Arrange
        const string toAddress = "test@example.com";
        _emailClient.SendEmail(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(true);

        // Act
        var result = _sut.SendWelcomeEmail(toAddress);

        // Assert
        result.Should().BeTrue();
        _emailClient.Received(1).SendEmail(toAddress, Arg.Any<string>(), Arg.Any<string>());
    }
}
```

---

## 8. Données de test et déterminisme

- Aucune variable magique dans l'appel ou les assertions.
- Préférer des `const` explicites pour les entrées et les attendus.
- Utiliser des données **réalistes**, **de bordure**, **erronées**, et **atypiques** selon la règle testée.
- Toute source pseudo-aléatoire doit être seedée.

Bogus :

```csharp
Randomizer.Seed = new Random(123456);
```

Temps et dates :

- Ne pas dépendre de `DateTime.Now`, `DateTime.UtcNow`, `DateOnly.FromDateTime(DateTime.Now)`, etc.
- Dans le code de production, injecter `TimeProvider`.
- Dans les tests, utiliser `FakeTimeProvider` quand il faut piloter le temps.

Culture et fuseau :

- Si le comportement dépend d'une culture, la fixer explicitement dans le test et la restaurer.
- Ne pas laisser un test dépendre de la culture de la machine ou de l'agent CI.

---

## 9. Verify, snapshots, fichiers versionnés

Pour les objets complexes et les contrats stables, utiliser `Verify.Xunit`.

Règles :

- Committer les fichiers `.verified.*`.
- Ne jamais commit les fichiers `.received.*`.
- Scrubber ou ignorer toutes les données non déterministes : timestamps, GUID non contrôlés, chemins machine, ordering instable.
- Utiliser Verify seulement si le snapshot reste **lisible et revuable**.
- Pour une comparaison simple sur quelques champs, préférer des assertions directes plutôt qu'un snapshot.

---

## 10. Cas particuliers fréquents dans ce repo

### ILogger<T>

- Si aucun assert sur les logs : `NullLogger<T>.Instance`.
- Ne pas transformer un logger en dépendance coûteuse à mocker sans besoin métier.

### HttpContext

- Utiliser `DefaultHttpContext` quand quelques propriétés concrètes suffisent.
- Sinon, substituer `IHttpContextAccessor` ou `HttpContext` autour de la surface virtuelle strictement nécessaire.

### IQueryable / DbSet / EF Core

- Pour des tests unitaires autour d'interfaces exposant un `IQueryable`, utiliser `MockQueryable.NSubstitute`.
- Pour la traduction SQL réelle, les includes, les comportements provider-specific ou les migrations, **ce n'est plus du test unitaire**.

### HttpClient

- Ne pas tester un vrai `HttpClient` en unitaire.
- Dans ce dépôt, préférer les abstractions déjà présentes, notamment les clients Refit, qui se substituent proprement.

---

## 11. Validation locale obligatoire

Après ajout ou modification de tests unitaires :

```powershell
# Suite .NET complète
dotnet test .\InfraFlowSculptor.slnx

# Projet de test ciblé
dotnet test .\tests\<TargetAssembly>.Tests\<TargetAssembly>.Tests.csproj

# Ciblage plus fin pendant l'itération
dotnet test .\tests\<TargetAssembly>.Tests\<TargetAssembly>.Tests.csproj --filter "FullyQualifiedName~<SutName>"
```

Règle de fin de tâche :

- Commencer par le projet touché pour itérer vite.
- Finir par `dotnet test .\InfraFlowSculptor.slnx` avant restitution.

---

## 12. Couverture et mutation

Couverture :

```powershell
dotnet test .\InfraFlowSculptor.slnx --collect:"XPlat Code Coverage"
```

Mutation testing :

```powershell
dotnet tool install --global dotnet-stryker
dotnet stryker --project .\tests\<TargetAssembly>.Tests\<TargetAssembly>.Tests.csproj
```

Règles :

- La couverture est un indicateur d'exécution, pas une preuve de qualité.
- Stryker est un contrôle avancé, utile pour des zones critiques ou quand la robustesse des tests est en doute.
- Sur ce dépôt, préférer **Stryker par projet de tests** plutôt qu'au niveau solution quand le scope est large.

---

## 13. Checklist rapide

- [ ] Projet de tests sous `tests/` et nommé `<TargetAssembly>.Tests`
- [ ] Un seul assembly ciblé par projet de tests
- [ ] Aucun test unitaire ajouté dans `InfraFlowSculptor.GenerationParity.Tests`
- [ ] Une classe/fichier de test par SUT
- [ ] Nommage `Given_When_Then(_Async)` respecté
- [ ] `sut` / `_sut` utilisés explicitement
- [ ] AAA visible
- [ ] Données déterministes, réalistes, sans magie
- [ ] Dépendances isolées correctement
- [ ] Aucun appel externe réel
- [ ] `dotnet test` projet puis solution exécuté