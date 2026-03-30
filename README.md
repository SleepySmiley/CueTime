# CueTime

CueTime e un'applicazione desktop WPF per la gestione dei tempi delle adunanze, la presentazione del timer su schermo esterno, il caricamento dei dati dal web e la gestione della musica pre e post adunanza.

## Proprieta e licenza

Questo progetto e di proprieta esclusiva di Christian Mattiolo.

Uso consentito:
- utilizzo personale o professionale
- modifica del codice per esigenze locali
- condivisione del progetto citando l'autore originale

Il software viene fornito "cosi com'e", senza garanzie.

## Prerequisiti

- Sistema operativo: Windows 10 o Windows 11
- SDK .NET: 10.0.x
- Runtime desktop: .NET Desktop Runtime 10
- IDE consigliato: Visual Studio 2022 con workload ".NET desktop development"
- Alternativa leggera: Visual Studio Code con estensioni C# e XAML

## Build

Compilazione da riga di comando:

```powershell
dotnet build CueTime.csproj -nologo
```

Pulizia completa dei file generati:

```powershell
dotnet clean CueTime.csproj -nologo
```

## Avvio

Esecuzione da riga di comando:

```powershell
dotnet run --project CueTime.csproj
```

Da Visual Studio:
- apri `CueTime.slnx`
- imposta `CueTime` come progetto di avvio
- premi `F5` oppure `Ctrl+F5`

## Configurazione

Le impostazioni applicative sono serializzate nel file JSON principale e includono:

- `Infrasettimanale`: giorno e orario dell'adunanza infrasettimanale
- `FineSettimana`: giorno e orario dell'adunanza del fine settimana
- `MonitorScelto`: monitor usato per la finestra timer esterna
- `PercorsoCartellaMusica`: cartella locale dei brani audio
- `DateVisitaSorvegliante`: settimane speciali usate per la logica delle parti
- `TemaSelezionato`: chiave del tema attivo
- `TemaPersonalizzato`: palette personalizzata dell'interfaccia

Le modifiche vengono raccolte localmente nella finestra Impostazioni e confermate solo al salvataggio.

## Percorsi di storage

CueTime salva i dati in queste cartelle:

- Impostazioni: `%APPDATA%\CueTime\settings.json`
- Salvataggi adunanze: `%LOCALAPPDATA%\CueTime\AdunanzeSalvate`
- Cache web: `%LOCALAPPDATA%\CueTime\cache`
- Log applicativi: `%LOCALAPPDATA%\CueTime\logs\app.log`
- Statistiche: `%LOCALAPPDATA%\CueTime\stats`

La cache contiene:
- HTML delle pagine gia scaricate
- link settimanali memorizzati localmente
- data ufficiale della commemorazione quando disponibile

## Architettura

Panoramica semplificata:

```text
App
|-- SettingsStore
|-- ThemeManager
`-- MainWindow
    |-- Adunanza
    |-- TimerLogics
    |-- FinestraTimer
    |-- PlayerMusicale
    `-- Impostazioni

WebPartsLoader
|-- WebFetcher
|-- WebPartsCache
|-- HtmlParteParser
`-- ParteFactory
```

Responsabilita principali:

- `App.xaml.cs`: bootstrap applicazione, caricamento e salvataggio impostazioni, applicazione tema
- `MainWindow`: coordinamento UI principale, navigazione tra parti, apertura finestre secondarie
- `Adunanza`: stato dell'adunanza corrente e collezione delle parti
- `TimerLogics`: logica del timer, pause e riprese, grafica sincronizzata con la parte corrente
- `FinestraTimer`: finestra esterna dedicata alla presentazione del timer
- `PlayerMusicale`: gestione playlist locale e riproduzione audio
- `Impostazioni`: editing impostazioni, monitor, date, tema, salvataggi adunanza
- `WebPartsLoader`: orchestrazione del caricamento web mantenendo invariata l'API pubblica
- `WebFetcher`: richieste HTTP con fallback limitato
- `WebPartsCache`: lettura e scrittura cache su disco
- `HtmlParteParser`: parsing HTML in dati grezzi
- `ParteFactory`: costruzione dei modelli `Parte`
- `SettingsStore`: persistenza atomica delle impostazioni
- `GestoreSalvataggi`: persistenza delle adunanze salvate manualmente
- `AppLogger`: logging su `Debug` e su file locale
- `GestoreStatisticheAdunanze`: archivio e calcolo delle statistiche storiche

## Sorgenti web e sicurezza

Il caricamento web usa una allowlist di host consentiti per i link letti dalla cache locale:

- `jw.org`
- `www.jw.org`
- `wol.jw.org`

I link cache fuori allowlist vengono ignorati e registrati nei log.

## Verifica

Al momento nel repository non sono presenti test automatici. La verifica minima consigliata e:

```powershell
dotnet build CueTime.csproj -nologo
dotnet run --project CueTime.csproj
```

## Troubleshooting

### La build fallisce dopo modifiche WPF o XAML

Esegui una pulizia completa e ricompila:

```powershell
dotnet clean CueTime.csproj -nologo
dotnet build CueTime.csproj -nologo
```

### Il caricamento web non restituisce le parti

- verifica la connettivita Internet
- controlla che i siti `jw.org` e `wol.jw.org` siano raggiungibili
- consulta `%LOCALAPPDATA%\CueTime\logs\app.log`
- se necessario svuota `%LOCALAPPDATA%\CueTime\cache`

### La musica non compare nella playlist

- verifica che `PercorsoCartellaMusica` punti a una cartella locale esistente
- sono accettate solo cartelle su unita locali fisse o rimovibili
- controlla che i file abbiano un'estensione audio supportata

### Le impostazioni sembrano corrotte o non leggibili

- controlla `%APPDATA%\CueTime\settings.json`
- in caso di file non valido, l'app tenta di creare un backup `settings.invalid-*.json`
- dopo il backup vengono ricreate impostazioni di default

### Il timer non appare sullo schermo corretto

- apri Impostazioni
- seleziona nuovamente il monitor desiderato
- salva e riapri la finestra timer se necessario

## Convenzioni di sviluppo

- mantenere separati trasporto HTTP, cache, parsing e costruzione modelli
- evitare stato globale mutabile dove non strettamente necessario
- registrare sempre gli errori rilevanti tramite `AppLogger`
