# Modul Identity

Modul Identity je platformski modul zadužen za registraciju korisnika, prijavu i izdavanje JWT-a (JSON Web Token). Sastoji se od tri projekta. Projekat `Identity` sadrži sam modul, projekat `Identity.Contracts` njegov kontrakt prema drugim modulima, a projekat `Identity.Tests` njegove integracione testove. Studenti ovaj modul koriste kroz njegove krajnje tačke i kroz kontrakt, ali ga ne menjaju i ne referenciraju projekat `Identity` iz svojih modula.

## Problem koji rešava

Skoro svaka krajnja tačka sistema mora da zna ko je prijavljeni korisnik. Podatak o korisniku je pri tome osetljiv, jer uz njega idu lozinka i uloge. Kada bi svaki modul sam vodio evidenciju korisnika, isti osetljivi podaci bi se ponavljali na više mesta i svaki tim bi morao da rešava iste bezbednosne probleme.

Zato o korisničkim nalozima brine jedan centralni modul. On proverava identitet korisnika pri prijavi i izdaje potpisan token u kome se nalaze identifikator korisnika, adresa elektronske pošte i uloge. Ostali moduli nikada ne pristupaju korisničkim nalozima. Prijavljenog korisnika poznaju samo preko vrednosti `UserId` koju kontroler čita iz tokena i prosleđuje aplikacionom sloju kao običan parametar. Kada modulu treba korisničko ime, na primer za pretragu korisnika ili prikaz imena uz identifikator, pita modul Identity kroz kontrakt, opisan u odeljku o projektu `Identity.Contracts`.

Posao oko tokena podeljen je na dva mesta. Modul Identity izdaje token, a projekat `Host.Api` konfiguriše proveru tokena za sve pristigle zahteve. Zahvaljujući toj podeli, funkcionalni moduli o tokenima ne znaju ništa. Njima je dovoljan atribut `[Authorize]` na kontroleru i vrednost `UserId` iz zahteva.

## Odstupanje od strukture modula

Funkcionalni moduli imaju pet projekata, po jedan za svaki sloj. Modul Identity ima jedan projekat sa celom logikom, uz zaseban projekat sa kontraktom.

Podela na pet projekata postoji da bi štitila domenska pravila i granice između timova. Modul Identity nema domenska pravila koja bi štitio, jer čitavu logiku naloga, lozinki i uloga preuzima gotova biblioteka ASP.NET Core Identity. Granica prema drugim timovima postoji samo na jednom mestu, a to je kontrakt, pa jedino on dobija zaseban projekat, kao kod funkcionalnih modula. Ostatak modula u celosti pripada platformskom timu i nijedan drugi modul ga ne referencira. Pet projekata bi u ovoj situaciji bila nepotrebna apstrakcija. Ovaj modul ujedno prikazuje snagu modular monolit arhitekture, koja dozvoljava da pratimo različite arhitekture na nivou pojedinačnih modula.

## Elementi projekta Identity

Projekat je podeljen na tri direktorijuma koji odgovaraju slojevima funkcionalnog modula.

### Api

Sadrži klasu `IdentityController` sa dve krajnje tačke. Krajnja tačka `POST /api/identity/register` prima adresu elektronske pošte, korisničko ime i lozinku, pravi novi nalog, dodeljuje mu ulogu `explorer` i vraća token. Adresa elektronske pošte i korisničko ime moraju biti jedinstveni. Krajnja tačka `POST /api/identity/login` proverava adresu elektronske pošte i lozinku i vraća token. Obe krajnje tačke su javne, jer im pristupaju korisnici koji još nemaju token. Kontroler direktno koristi klasu `UserManager` iz biblioteke ASP.NET Core Identity, koja vrši ulogu aplikacionog servisa.

Sadrži i klasu koja definiše metodu `AddIdentityControllers` kojom `Host.Api` uključuje kontrolere ovog projekta, po istom obrascu kao kod funkcionalnih modula.

### Core

Sadrži klase koje opisuju korisnika i token. Klasa `ApplicationUser` predstavlja korisnika i nasleđuje gotovu klasu `IdentityUser`, bez dodatnih polja. Klasa `JwtSettings` sadrži podešavanja tokena koja se čitaju iz konfiguracije, poput ključa za potpisivanje i roka važenja. Klasa `JwtTokenFactory` od korisnika i njegovih uloga pravi potpisan token. Token nosi identifikator korisnika u polju `sub`, adresu elektronske pošte u polju `email` i po jedno polje za svaku ulogu.

Ovde su i DTO klase koje krajnje tačke primaju i vraćaju. Klase `RegisterDto` i `LoginDto` opisuju zahteve, a klasa `AccessTokenDto` odgovor sa tokenom.

Klasa `IdentityApi` implementira kontrakt `IIdentityApi` iz projekta `Identity.Contracts`. Kao i kontroler, direktno koristi klasu `UserManager` i projektuje korisnike u strukturu `UserSummaryDto`.

### Infrastructure

Sadrži pristup bazi i uključivanje modula u aplikaciju. Klasa `IdentityModuleDbContext` nasleđuje gotov kontekst biblioteke ASP.NET Core Identity i smešta njegove tabele u šemu `identity`, po pravilu da svaki modul ima svoju šemu. Direktorijum `Migrations` sadrži migracije te šeme.

Metoda `AddIdentityModule` registruje sve delove modula i jedino je mesto koje `Host.Api` poziva pored metode `AddIdentityControllers`. Klasa `IdentityModuleInitializer` se izvršava pri pokretanju aplikacije. Ona primenjuje migracije i upisuje uloge `administrator` i `explorer` ako već ne postoje, pa aplikacija posle pokretanja uvek ima spremnu šemu i uloge.

## Projekat Identity.Contracts

Projekat sadrži kontrakt kroz koji drugi moduli dobijaju korisnička imena, i ne referencira nijedan drugi projekat. Interfejs `IIdentityApi` ima dve metode. Metoda `SearchByUsernameAsync(term, limit)` vraća najviše `limit` korisnika čije korisničko ime sadrži zadati tekst, bez obzira na velika i mala slova. Metoda `GetByIdsAsync(userIds)` vraća korisnička imena za zadate identifikatore. Obe vraćaju strukturu `UserSummaryDto` sa identifikatorom i korisničkim imenom, i ništa više: adresa elektronske pošte, lozinka i uloge ne napuštaju modul.

Modul kome treba korisničko ime referencira ovaj projekat iz svog aplikacionog sloja i prima `IIdentityApi` kroz konstruktor svoje upitne klase, kao i kontrakt bilo kog drugog modula. Projekat `Identity` i dalje ne referencira nijedan modul.

## Projekat Identity.Tests

Projekat sadrži integracione testove koji šalju prave HTTP zahteve krajnjim tačkama registracije i prijave i proveravaju odgovore, uključujući sadržaj vraćenog tokena. Ovo je jedini projekat koji te dve krajnje tačke testira, jer su jedino ovde one predmet testa. Testovi ostalih modula prijavljenog korisnika dobijaju direktno od test infrastrukture, bez prolaska kroz modul Identity.

Kontrakt nema krajnju tačku, pa ga klasa `IdentityContractTests` testira kroz ulaznu tačku koju koriste drugi moduli: dobija `IIdentityApi` iz kontejnera zavisnosti pokrenute aplikacije i poziva njegove metode nad početnim podacima.

Početni podaci ovog projekta imaju jednu posebnost. U funkcionalnim modulima početni podaci nastaju pozivom domenskih konstruktora. Korisnički nalog se u pravom radu pravi kroz klasu `UserManager`, koja usput računa heš lozinke i normalizovane kolone. Mehanizam za vraćanje baze na početno stanje upisuje redove direktno u bazu i taj korak zaobilazi, pa klasa `UserSeed` te vrednosti popunjava sama. Identifikator korisnika dolazi iz klase `WellKnownUsers`, tako da testovi svih modula govore o istim korisnicima.
