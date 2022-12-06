# WebNett
WebServer sviluppato in C# utilizzando le Socket.


## Installazione:
1) Clonare la repository
```
git clone https://github.com/Ivnoo/webnett
```
2) Aver installato .NET 4.7.2 (minimo)
3) Aprire il file "WebServer.sln" con VisualStudio 2022
4) Avviare la compilazione con il tasto "Start"

## Funzionamento:
1) Una volta avviato il programma, cliccare il pulsante a forma di cartella e selezionare il percorso contenente i file del vostro sito web. (**ATTENZIONE: Il percorso selezionato _DEVE_ contenere almeno un file HTML, la pagina principale _DEVE_ essere nominata _index.html_.**)
2) Selezionare l'indirizzo IP che si vuole utilizzare tramite il menù a tendina sulla destra. (**Solo IPv4**)
3) Selezionare la porta sulla quale il server sarà in ascolto (**default: 80**).
4) Selezionare il numero massimo di connessioni contemporanee (**default: 1**).
5) Premere il pulsante **START** per avviare il server. (**Se richiesto, permettere la comunicazione su rete privata da Firewall Windows**)
6) Premere il pulsante **STOP** per fermare il server.

## Note
Il Server gestisce **_ESCLUSIVAMENTE_** richieste di tipo **GET** per file **HTML, CSS, JS e Immagini (PNG, ICO, JPG, JPEG)**, altre tipologie di richieste verranno ignorate.
Il Server non supporta integrazione con **PHP**, mostrerà solamente il contenuto del file.
