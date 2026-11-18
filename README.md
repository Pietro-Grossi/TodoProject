# TodoProject

## Obiettivo del progetto

API REST in C# e ASP.NET Core per gestire una lista di attività.

## Funzionalità

- Creazione di un task
- Aggiornamento di un task
- Eliminazione di un task completato
- Elenco dei task con filtro per stato

I dati sono conservati in memoria. Le modifiche si perdono al riavvio dell’applicazione.

## Esecuzione del progetto

Installa .NET 10 SDK ed esegui i comandi dalla radice del repository.

### Avvio locale

```powershell
dotnet run --project src/TodoProject/TodoProject.csproj
```

### Test

```powershell
dotnet test tests/TodoProject.Tests/TodoProject.Tests.csproj
```
