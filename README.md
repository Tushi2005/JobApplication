# JobTracker – Backend

ASP.NET Core 10 REST API állásjelentkezések kezeléséhez. ASP.NET Core Identity alapú hitelesítés, Entity Framework Core + PostgreSQL adatbázis, Docker alapú deployment GitHub Actions CI/CD pipeline-nal.

## Technológiai stack

- **ASP.NET Core 10** – Web API
- **Entity Framework Core** – Code First, migrációk
- **PostgreSQL** – adatbázis (éles és fejlesztői környezetben egyaránt)
- **ASP.NET Core Identity** – felhasználókezelés, jelszó hashelés
- **JWT Bearer Authentication** – hitelesítés és azonosítás
- **Swagger UI** – API dokumentáció és tesztelés
- **Docker + Docker Compose** – konténerizáció
- **GitHub Actions** – CI/CD pipeline self-hosted runnerrel

## Projekt struktúra

```
JobApplication/
├── Controllers/
│   ├── ApplicationsController.cs   # CRUD végpontok jelentkezésekhez
│   └── AuthController.cs           # Regisztráció és bejelentkezés (Identity)
├── Services/
│   └── Applications/
│       ├── IApplicationService.cs
│       └── ApplicationService.cs
├── Models/
│   ├── Application.cs
│   ├── ApplicationStatus.cs        # Enum: Sent, InterviewScheduled, stb.
│   └── ApplicationUser.cs          # IdentityUser leszármazott
├── DTOs/
│   ├── Application/                # Create, Update, Patch, Response DTO-k
│   └── Auth/                       # Login, Register, Response DTO-k
├── Mappers/
│   └── ApplicationMapper.cs        # Extension method: Application → DTO
├── Data/
│   └── AppDbContext.cs             # IdentityDbContext<ApplicationUser>
├── Migrations/
├── Dockerfile
├── docker-compose.yml
└── .github/workflows/deploy.yml    # CI/CD pipeline
```

## Architektúra

### Rétegek

A projekt a Separation of Concerns elvét követi:

- **Controller réteg** – kizárólag HTTP kommunikáció, bemeneti validáció, válasz formázás
- **Service réteg** – üzleti logika, adatbázis műveletek
- **Mapper réteg** – entitás → DTO átalakítás extension methodok segítségével
- **Data réteg** – `AppDbContext`, EF Core konfiguráció

Az `IApplicationService` az ASP.NET Core DI konténerbe Scoped élettartammal van regisztrálva. A hitelesítést az ASP.NET Core Identity `UserManager<ApplicationUser>` és `SignInManager<ApplicationUser>` szolgáltatásai kezelik közvetlenül az `AuthController`-ben.

### Adatmodellek

**ApplicationUser** (IdentityUser leszármazott)
| Mező | Típus | Leírás |
|------|-------|--------|
| Id | string (GUID) | Elsődleges kulcs (Identity) |
| Email | string | Egyedi, kötelező (Identity) |
| PasswordHash | string | Identity által kezelt hash |
| UserName | string | Identity (email-lel megegyezik) |
| FullName | string | Kötelező, egyedi mező |
| CreatedAt | DateTime | Létrehozás dátuma |

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
| POST | `/api/auth/register` | Regisztráció, JWT token visszaadása |
| POST | `/api/auth/login` | Bejelentkezés, JWT token visszaadása |

### Jelentkezések (JWT szükséges)

| Metódus | Végpont | Leírás |
|---------|---------|--------|
| GET | `/api/applications` | Saját jelentkezések listája |
| GET | `/api/applications/{id}` | Egy jelentkezés részletei |
| GET | `/api/applications/companies` | Saját cégnevek (autocomplete) |
| GET | `/api/applications/positions` | Saját pozíciók (autocomplete) |
| POST | `/api/applications` | Új jelentkezés rögzítése |
| PUT | `/api/applications/{id}` | Jelentkezés teljes frissítése |
| PATCH | `/api/applications/{id}` | Státusz frissítése |
| DELETE | `/api/applications/{id}` | Jelentkezés törlése |

Minden védett végpont kizárólag a bejelentkezett felhasználó saját adatait adja vissza – a userId a JWT tokenből kerül kiolvasásra.

## Hitelesítés

Az alkalmazás ASP.NET Core Identity + JWT kombinációt használ. A flow:

1. Regisztrációkor az `UserManager.CreateAsync()` kezeli a jelszó hashelését és a felhasználó létrehozását
2. Bejelentkezéskor a `SignInManager.CheckPasswordSignInAsync()` validálja a jelszót
3. Sikeres bejelentkezés után a szerver JWT tokent állít elő (Claims: userId, email, fullName)
4. A kliens minden kéréshez csatolja: `Authorization: Bearer <token>`
5. Az ASP.NET Core JwtBearer middleware automatikusan validálja (issuer, audience, lifetime, signature)
6. Hibás bejelentkezésnél a szerver nem árulja el, hogy az email vagy a jelszó volt-e helytelen

## Lokális fejlesztés

### Követelmények
- .NET 10 SDK
- Docker Desktop

### Indítás

**1. Lokális PostgreSQL indítása Docker-rel:**
```bash
docker compose -f docker-compose.dev.yml up -d
```

**2. Migrációk futtatása:**
```bash
dotnet ef database update
```

**3. Backend indítása:**
```bash
dotnet run
```

Az API elérhető: `http://localhost:5000`  
Swagger UI: `http://localhost:5000/swagger`

### Környezeti változók (fejlesztői)

Az `appsettings.Development.json` fájlban (git által figyelmen kívül hagyva):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=jobtracker_dev;Username=adam;Password=..."
  },
  "Jwt": {
    "Key": "minimum-32-karakteres-titkos-kulcs",
    "Issuer": "JobApplication",
    "Audience": "JobApplicationUsers",
    "ExpiryInDays": 7
  }
}
```

## CI/CD Pipeline

A GitHub Actions workflow (`deploy.yml`) minden master ágra történő push esetén automatikusan:

1. Buildeli a Docker image-t
2. Feltölti Docker Hub-ra (`adamtuska/jobtracker-backend:latest`)
3. A self-hosted runneren keresztül deploy-olja a szerverre `docker compose`-zal

Az érzékeny adatok (DB jelszó, JWT kulcs) GitHub Secrets-ben vannak tárolva, és környezeti változóként kerülnek be a konténerbe.

### Migrációk production környezetben

Az alkalmazás induláskor automatikusan futtatja a pending EF Core migrációkat (`db.Database.Migrate()`), így deploy után az adatbázis séma automatikusan frissül és a meglévő adatok megmaradnak. Nem szükséges manuálisan futtatni a `dotnet ef database update` parancsot a szerveren.

## Verziókezelés

A fejlesztés feature branch workflow szerint zajlott:

- `feat/models` – adatmodellek
- `feat/ef-setup` – Entity Framework és adatbázis konfiguráció
- `feat/services` – service réteg
- `feat/controllers` – controller réteg és JWT hitelesítés
- `fix/route-collision-400` – útvonal ütközés javítása
- `refactor/postgreDb` – átállás MSSQL-ről PostgreSQL-re

Commit üzenetek a Conventional Commits konvenció szerint készültek.
