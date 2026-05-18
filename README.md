# JobTracker – Backend

ASP.NET Core 10 REST API állásjelentkezések kezeléséhez. ASP.NET Core Identity alapú hitelesítés (email/jelszó és Google OAuth), Bearer Token + Refresh Token, Entity Framework Core + PostgreSQL, Docker alapú deployment GitHub Actions CI/CD pipeline-nal, Cloudflare Tunnel-en keresztül.

**Éles URL:** `https://jobtracker-api.adamcloud.hu`

## Technológiai stack

- **ASP.NET Core 10** – Web API, Minimal API endpoints
- **Entity Framework Core** – Code First, migrációk, automatikus alkalmazás induláskor
- **PostgreSQL** – adatbázis
- **ASP.NET Core Identity** – felhasználókezelés, jelszó hashelés
- **Bearer Token Authentication** – 15 perces access token, 7 napos refresh token
- **Google OAuth 2.0** – külső bejelentkezés, automatikus regisztráció
- **Newtonsoft.Json** – enum string szerializáció
- **Serilog** – strukturált naplózás
- **Swagger UI** – API dokumentáció és tesztelés
- **Rate Limiting** – brute force védelem login végpontokon
- **Docker + Docker Compose** – konténerizáció
- **GitHub Actions** – CI/CD pipeline self-hosted TrueNAS runnerrel
- **Cloudflare Tunnel** – HTTPS proxy a helyi szerverre

## Projekt struktúra

```
JobApplication/
├── Controllers/
│   └── ApplicationsController.cs   # CRUD végpontok jelentkezésekhez
├── Extensions/
│   ├── EndpointExtensions.cs       # Minimal API: auth, Google OAuth, /api/me
│   ├── DatabaseExtensions.cs       # EF Core + provider alapú konfig
│   ├── IdentityExtensions.cs       # Identity, Bearer token, Google auth beállítások
│   ├── CorsExtensions.cs           # CORS policy (Angular frontend)
│   ├── SwaggerExtensions.cs
│   ├── RateLimitingExtensions.cs
│   └── SerilogExtensions.cs
├── Services/
│   └── Applications/
│       ├── IApplicationService.cs
│       └── ApplicationService.cs
├── Models/
│   ├── Application.cs
│   ├── ApplicationStatus.cs        # Enum: Sent, InterviewScheduled, stb.
│   └── ApplicationUser.cs          # IdentityUser leszármazott
├── DTOs/
│   └── Application/                # Create, Update, Response DTO-k
├── Mappers/
│   └── AutoMapperApplication.cs    # AutoMapper profil
├── Data/
│   └── AppDbContext.cs             # IdentityDbContext<ApplicationUser>
├── Exceptions/
│   └── GlobalExceptionHandler.cs
├── Migrations/
├── Dockerfile
├── docker-compose.yml
└── .github/workflows/deploy.yml    # CI/CD pipeline
```

## Architektúra

### Rétegek

- **Controller réteg** – HTTP kommunikáció, bemeneti validáció, válasz formázás
- **Minimal API endpoints** – hitelesítés, Google OAuth callback, `/api/me`
- **Service réteg** – üzleti logika, adatbázis műveletek
- **AutoMapper** – entitás ↔ DTO átalakítás
- **Data réteg** – `AppDbContext`, EF Core konfiguráció

### Adatmodellek

**ApplicationUser** (IdentityUser leszármazott)
| Mező | Típus | Leírás |
|------|-------|--------|
| Id | string (GUID) | Elsődleges kulcs (Identity) |
| Email | string | Egyedi, kötelező |
| PasswordHash | string | Identity által kezelt hash |
| UserName | string | Email-lel megegyezik |
| FullName | string? | Google OAuth esetén a profil nevéből töltődik |

**Application**
| Mező | Típus | Leírás |
|------|-------|--------|
| Id | int | Elsődleges kulcs |
| UserId | string | Idegen kulcs (AspNetUsers) |
| CompanyName | string | Kötelező |
| Position | string | Kötelező |
| Status | ApplicationStatus | Enum (6 státusz) |
| AppliedAt | DateTime | Jelentkezés dátuma |
| InterviewAt | DateTime? | Interjú időpontja |
| JobUrl | string? | Hirdetés linkje |
| Notes | string? | Megjegyzések |
| CreatedAt | DateTime | Létrehozás dátuma |
| UpdatedAt | DateTime | Módosítás dátuma |

**ApplicationStatus enum**
```
Sent | InterviewScheduled | SecondRound | Accepted | Rejected | NoResponse
```

## API végpontok

### Hitelesítés (nyilvános)

| Metódus | Végpont | Leírás |
|---------|---------|--------|
| POST | `/login` | Bejelentkezés, access + refresh token visszaadása |
| POST | `/register` | Regisztráció (MapIdentityApi) |
| POST | `/refresh` | Access token megújítása refresh tokennel |
| GET | `/api/auth/google` | Google OAuth flow indítása |
| GET | `/api/auth/google/callback` | Google callback kezelése, token generálás |
| POST | `/api/auth/register` | Regisztráció névvel (saját endpoint) |

### Bejelentkezett felhasználó

| Metódus | Végpont | Leírás |
|---------|---------|--------|
| GET | `/api/me` | Bejelentkezett felhasználó adata (email, fullName) |

### Jelentkezések (Bearer token szükséges)

| Metódus | Végpont | Leírás |
|---------|---------|--------|
| GET | `/api/applications` | Saját jelentkezések listája |
| GET | `/api/applications/{id}` | Egy jelentkezés részletei |
| GET | `/api/applications/companies` | Saját cégnevek (autocomplete) |
| GET | `/api/applications/positions` | Saját pozíciók (autocomplete) |
| POST | `/api/applications` | Új jelentkezés rögzítése |
| PUT | `/api/applications/{id}` | Jelentkezés teljes frissítése |
| PATCH | `/api/applications/{id}` | Részleges frissítés (JSON Patch RFC 6902) |
| DELETE | `/api/applications/{id}` | Jelentkezés törlése |

Minden védett végpont kizárólag a bejelentkezett felhasználó saját adatait adja vissza.

## Hitelesítés

### Email/jelszó flow

1. `POST /login` – `SignInManager` validálja a jelszót
2. Az ASP.NET Core Identity Bearer Token middleware kiállít egy **15 perces access tokent** és egy **7 napos refresh tokent**
3. A kliens minden kéréshez csatolja: `Authorization: Bearer <token>`
4. Lejárat után a kliens `POST /refresh`-sel kér új tokenpárt

### Google OAuth flow

1. A felhasználó a `/api/auth/google`-ra navigál – `prompt=select_account` biztosítja a fiókválasztót
2. A backend Google-ra irányítja a felhasználót a regisztrált callback URI-val
3. Google visszahív `/signin-google`-ra, az ASP.NET Core feldolgozza
4. A `/api/auth/google/callback` handler:
   - Ha nincs még fiókja: automatikusan létrehozza a Google profil adataiból
   - Access token + refresh token generálása
   - Átirányítás a frontendre URL paraméterként: `/auth/callback?accessToken=...&refreshToken=...`

### Cloudflare Tunnel és proxy beállítás

Az app Cloudflare Tunnel mögött fut, ezért a `UseForwardedHeaders` middleware szükséges – enélkül az app HTTP-nek látja a bejövő kéréseket, és a Google OAuth callback URI-t rosszul konstruálja. A Docker hálózatból érkező proxy (172.x.x.x) explicit engedélyezve van.

## Lokális fejlesztés

### Követelmények
- .NET 10 SDK
- PostgreSQL (lokálisan vagy Docker-ben)

### Indítás

**1. Fejlesztői konfiguráció** (`appsettings.Development.json`, gitignore-ban):
```json
{
  "DatabaseProvider": "Postgres",
  "ConnectionStrings": {
    "Postgres": "Host=localhost;Port=5432;Database=jobtracker_dev;Username=adam;Password=..."
  },
  "Authentication": {
    "Google": {
      "ClientId": "...",
      "ClientSecret": "..."
    }
  },
  "FrontendUrl": "https://localhost:4200"
}
```

**2. Migrációk és indítás:**
```bash
dotnet ef database update
dotnet run
```

Az API elérhető: `https://localhost:7194`  
Swagger UI: `https://localhost:7194/swagger`

## CI/CD Pipeline

A GitHub Actions workflow (`deploy.yml`) minden `master` ágra történő push esetén automatikusan:

1. Buildeli a Docker image-t (közvetlenül `.csproj`-ból, nem a `.slnx` solution-ből)
2. Feltölti Docker Hub-ra (`adamtuska/jobtracker-backend:latest`)
3. A self-hosted TrueNAS runneren `docker compose pull` + `docker compose up -d`

Az érzékeny adatok GitHub Secrets-ben vannak tárolva:

| Secret | Env változó a konténerben |
|--------|--------------------------|
| `DB_CONNECTION` | `ConnectionStrings__Postgres` |
| `JWT_KEY` | `Jwt__Key` |
| `GOOGLE_CLIENT_ID` | `Authentication__Google__ClientId` |
| `GOOGLE_CLIENT_SECRET` | `Authentication__Google__ClientSecret` |
| `FRONTEND_URL` | `FrontendUrl` |

A `DatabaseProvider=Postgres` env változó a `docker-compose.yml`-ben hardkódelve van (nem titok).

### Migrációk production környezetben

Az app induláskor automatikusan futtatja a pending EF Core migrációkat (`db.Database.Migrate()`), így deploy után az adatbázis séma automatikusan frissül.

## Verziókezelés

Feature branch workflow, Conventional Commits konvenció szerint:

- `feat/models` – adatmodellek
- `feat/ef-setup` – Entity Framework és adatbázis konfiguráció
- `feat/services` – service réteg
- `feat/controllers` – controller réteg
- `feat/google-auth` – Google OAuth integráció, Bearer Token, Refresh Token
- `refactor/postgreDb` – átállás PostgreSQL-re
- `fix/docker-build` – Dockerfile javítás (.csproj explicit publish)
- `fix/cloudflare-proxy` – ForwardedHeaders middleware Cloudflare Tunnel-hez
