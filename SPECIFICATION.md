# Špecifikácia - gcron

## Popis

Cieľom je napísať vlastný implementáciu unix utility `cron`. Pozostáva z dvoch častí, scheduler, ktorý beží na pozadí a vykonáva používateľom zadané príkazy v zadaných časoch a command line tool na editovanie konfigurácie.

## Features

- Podpora pre základnú syntax `crontab` ako nižšie. `*` - matchne každý čas, `,` - oddeľuje viac záznamov v jedno poli, `-` - definuje rozsah hodnôt pre dené pole.

```text
# * * * * * <command to execute>
# | | | | |
# | | | | day of the week (0–7) (Sunday to Saturday; 7 is also Sunday)
# | | | month (1–12)
# | | day of the month (1–31)
# | hour (0–23)
# minute (0–59)
```

- Chcel by som aby hlavný scheduler šetril zdrojmi, to znamená, že nebude celý čas aktívne testovať, či je správny čas na spustenie príkazu, ale bude pasívne čakať až do toho času. Na zmenu konfigurácie ho bude notifikovať tool na editovanie konfigurácie.
- Primárne by som chcel aby fungoval správne pre Linux, podporu pre Windows by som pridal ak na to vyjde čas.
- Na editovanie konfigurácie sa použije userov preferovaný text editor.
- Nebude podporovať konfigurácie viacerých používateľov na jedno systéme. Každý používateľ na danom systéme si bude musieť spustiť vlastnú inštanciu.
- Čas v konfigurácii bude interpretovaný v časovom pásme aktuálneho používateľa.
