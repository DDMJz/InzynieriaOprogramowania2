# InzynieriaOprogramowania2

## Opis
Repozytorium zawiera aplikację do zarządzania flotą aut napisaną w C#, z interfejsem webowym w Razor oraz bazą danych MySQL
## Obsługa Testów

Aby uruchomić testy w katalogu FleetManager odpalamy polecenie:
```
dotnet test FleetManager.Tests/FleetManager.Tests.csproj
```
Pipeline:
```
dotnet test FleetManager.Tests/FleetManager.Tests.csproj --collect:"XPlat Code Coverage” --results-directory ./TestResults
```

## Obsługa Git

### Klonowanie repozytorium
Aby pobrać projekt na swój komputer, użyj:

```
git clone <adres_repozytorium>
```

### Tworzenie nowego brancha

Przed rozpoczęciem pracy najlepiej utworzyć nowy branch:

```bash
git checkout -b nazwa-brancha
```

### Dodawanie plików do commita

Aby dodać wszystkie zmienione pliki:
```bash
git add .
```

Zapisywanie zmian
```bash
git commit -m "Krótki opis zmian"
```

Wysyłanie zmian do repozytorium

```bash
git push origin nazwa-brancha
```


Pobieranie najnowszych zmian
```bash
git pull origin main
```

