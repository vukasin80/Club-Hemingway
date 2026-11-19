# ☕ Caffe Shop Management System — v2.0

Windows Forms aplikacija za upravljanje kafićem, pisana u C# (.NET 8).

## Struktura projekta

```
CaffeShop/
├── CaffeShop.csproj
├── Program.cs                    ← Entry point (Login → MainForm)
├── Database/
│   └── DatabaseManager.cs       ← SQLite init, hash lozinki, seed
├── Models/
│   └── Models.cs                ← User, Session, MenuItem, Employee, Order, OrderItem
├── Repositories/
│   ├── Repositories.cs          ← MenuRepository, EmployeeRepository, OrderRepository
│   ├── UserRepository.cs        ← Login, CRUD korisnici, promena lozinke
│   └── ReportsRepository.cs     ← DailyReport, TopItem, EmployeeReport, Summary
└── Forms/
    ├── LoginForm.cs             ← Prijava sa blokadom posle 5 pokušaja
    ├── MainForm.cs              ← Sidebar navigacija + Dashboard + Odjava
    ├── OrdersPanel.cs           ← Narudžbine + štampanje računa
    ├── MenuPanel.cs             ← CRUD meni
    ├── EmployeesPanel.cs        ← CRUD zaposleni
    ├── ReportsPanel.cs          ← Izveštaji + PDF + Excel export
    ├── UsersPanel.cs            ← Upravljanje korisnicima (samo Admin)
    └── SrpskiDijalog.cs        ← Custom Da/Ne dijalog
```

## Kako pokrenuti

1. Otvori `CaffeShop.csproj` u **Visual Studio 2022**
2. NuGet automatski preuzima: `System.Data.SQLite`
3. **F5** ili **Ctrl+F5**

### Demo nalozi (kreirani automatski pri prvom pokretanju)

| Korisnik | Lozinka    | Uloga   | Pristup                          |
|----------|------------|---------|----------------------------------|
| vule    | admin123   | Admin   | Sve funkcije                     |
| konobar1 | konobar123 | Konobar | Narudžbine, Meni, Zaposleni      |

---

## Funkcionalnosti

### 🔐 Sistem korisnika i logovanje
- Login forma sa hashovanim lozinkama (SHA-256 + salt)
- Blokada posle **5 neuspešnih pokušaja**
- **2 nivoa pristupa:** Admin vidi sve, Konobar ne vidi Izveštaje i Korisnike
- Odjava sa potvrdom + mogućnost prijave drugog korisnika bez restarta
- Admin može: dodati/izmeniti/deaktivirati korisnike, promeniti lozinke

### 🏠 Dashboard
- Prihod danas, broj otvorenih/zatvorenih narudžbina
- Lista svih današnjih narudžbina
- Sat u realnom vremenu

### 📋 Narudžbine + Štampanje
- Kreiranje po stolu i konobaru
- Dodavanje/uklanjanje artikala, automatski obračun
- **Naplata** → pita da li štampati račun
- **Štampanje računa** u svakom trenutku (dugme 🖨️)
- PrintPreview dialog + Windows štampač
- Format: 80mm termalni račun (Artikal, količina × cena, ukupno, fiskalni br.)

### ☕ Meni
- CRUD artikla (naziv, kategorija, cena, opis)
- Uključivanje/isključivanje dostupnosti jednim klikom

### 👥 Zaposleni
- CRUD, soft delete (deaktivacija), filter aktivan/svi

### 📊 Izveštaji (samo Admin)
- Filter po datumskom opsegu
- **Dnevni pregled** — po danima u mesecu
- **Top artikli** — najprodavaniji po količini i prihodu
- **Po konobaru** — narudžbine i prihod po zaposlenom
- 4 summary kartice (ukupan prihod, narudžbine, artikli, prosek)

### 📤 Export
- **Export PDF** → HTML izveštaj koji se otvara u browseru (Print → Save as PDF)
- **Export Excel** → CSV sa BOM-om (direktno otvara u Excelu, srpski karakteri OK)

### 🔑 Upravljanje korisnicima (samo Admin)
- Dodaj/izmeni/deaktiviraj korisnika
- Promena lozinke za bilo koji nalog
- Ne može deaktivirati sopstveni nalog

---

## Tehnologije

| | |
|---|---|
| Jezik | C# 12 |
| Framework | .NET 8 |
| UI | Windows Forms |
| Baza | SQLite (System.Data.SQLite) |
| Sigurnost | SHA-256 + salt hashing |
| Štampa | System.Drawing.Printing (PrintDocument, PrintPreviewDialog) |

---

## Moguća proširenja
- [ ] Grafički prikaz statistike (bar/line chart)
- [ ] Upravljanje zalihama
- [ ] iTextSharp za pravi PDF export
- [ ] QR kod na računu
- [ ] Višejezična podrška
