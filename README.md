# TodoProject

## Obiettivo del progetto

API REST in C# e ASP.NET Core per gestire una lista di attività.

## Funzionalità

- Creazione di un task
- Aggiornamento del titolo e dello stato di un task
- Eliminazione di un task completato
- Elenco dei task con filtri per stato e testo del titolo

I dati sono conservati in memoria. Le modifiche si perdono al riavvio dell’applicazione.

### Funzionalità aggiuntive

- Pulizia automatica dei task completati ogni 15 minuti, con prima esecuzione dopo 15 minuti dall’avvio dell’applicazione.



## Esecuzione del progetto

Installa .NET 10 SDK ed esegui i comandi dalla radice del repository.

### Avvio locale

Avvia il profilo di sviluppo HTTP sulla porta 5298.

```powershell
dotnet run --project src/TodoProject/TodoProject.csproj --launch-profile http --urls http://localhost:5298
```



### Test delle API con Swagger

Con l'applicazione avviata, apri [Swagger UI](http://localhost:5298/swagger).
Seleziona un endpoint, premi **Try it out**, inserisci i parametri o il corpo JSON e premi **Execute** per inviare la richiesta e leggere la risposta.

### Test automatici

```powershell
dotnet test tests/TodoProject.Tests/TodoProject.Tests.csproj
```
