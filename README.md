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

## API

| Metodo | Endpoint | Descrizione | Parametri o corpo JSON | Risposte |
|---|---|---|---|---|
| GET | `/api/tasks` | Elenca e filtra i task. | Query facoltative `isCompleted` e `search` | 200, 400 |
| GET | `/api/tasks/{id}` | Recupera un task per ID. | ID nel percorso | 200, 404 |
| POST | `/api/tasks` | Crea un task aperto. | `{"title":"Nuovo task"}` | 201, 400 |
| PATCH | `/api/tasks/{id}/title` | Modifica il titolo. | `{"title":"Nuovo titolo"}` | 204, 400, 404 |
| PATCH | `/api/tasks/{id}/status` | Completa o riapre un task. | `{"isCompleted":true}` | 204, 400, 404 |
| DELETE | `/api/tasks/{id}` | Elimina un task completato. | ID nel percorso | 204, 404, 409 |

### Parametri e vincoli

- `id` è un intero. Se il task richiesto non esiste, l'API restituisce `404`.
- `title` è obbligatorio, deve contenere almeno una lettera o una cifra e può avere al massimo 200 caratteri dopo la rimozione della spaziatura ai bordi. Sono vietati caratteri di controllo e separatori di riga o paragrafo interni.
- `isCompleted` è obbligatorio nel corpo del PATCH dello stato e accetta `true` o `false`. Come filtro del GET è facoltativo e, se omesso, include entrambi gli stati.
- `search` è facoltativo e cerca nel titolo senza distinguere maiuscole e minuscole. La spaziatura ai bordi viene rimossa. Un valore vuoto o di soli spazi equivale a nessun filtro. I filtri specificati si applicano insieme.

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
