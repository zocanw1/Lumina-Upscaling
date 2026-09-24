# Lumina-Upscaling

Aplikasi Windows untuk memperjelas garis dan detail gambar anime secara 100% offline. Dilengkapi antarmuka modern dengan fitur perbandingan sebelum-sesudah (before-after slider).

## Fitur

- **Comparison Slider** — Geser untuk membandingkan gambar asli dan hasil AI
- **Real-ESRGAN Anime V1** — Upscale 4x
- **Real-ESRGAN Anime V2** — Upscale 2x dan 4x
- **Real-ESRGAN Anime V3** — Upscale 2x, 3x, dan 4x
- **Input**: PNG, JPG/JPEG, dan WebP
- **Output**: PNG, JPG, dan WebP
- **Drag-and-drop** langsung ke area preview
- **Zoom** 100%–400% (tombol + Ctrl + scroll)
- **Batal** proses kapan saja
- **Save As** untuk menyimpan hasil
- **100% Offline** — Tidak butuh internet, API, cloud, atau akun

## Prasyarat

- Windows 10 / 11
- .NET 8 SDK (untuk build dari source)
- GPU dengan Vulkan driver (disarankan untuk performa terbaik)

## Cara Pakai

### 1. Jalankan Aplikasi
Buka folder `artifacts/StellaUpscaling-win-x64`, lalu jalankan **StellaUpscaling.exe**.

### 2. Pilih atau Tarik Gambar
- Klik tombol **"Pilih Gambar"**, atau
- **Tarik (drag)** file gambar langsung ke area preview
- Format yang didukung: **PNG, JPG, JPEG, WebP**

### 3. Atur Model & Skala
- **Model**: Pilih salah satu dari tiga model AI yang tersedia
  - **Real-ESRGAN Anime V1** → hanya 4x
  - **Real-ESRGAN Anime V2** → 2x atau 4x
  - **Real-ESRGAN Anime V3** → 2x, 3x, atau 4x (paling fleksibel)
- **Skala**: Pilih berapa kali lipat gambar di-perbesar (sesuai model yang dipilih)
- **Format Output**: Pilih PNG (default), JPG, atau WebP

### 4. Klik "Upscale Sekarang"
- Progress bar akan menunjukkan proses
- Waktu pemrosesan ditampilkan di bawah
- Klik **"Batalkan"** untuk menghentikan proses

### 5. Bandingkan Hasil
- **Geser slider** di tengah untuk membandingkan gambar asli vs hasil AI
- **Zoom** ke 400% untuk melihat detail dengan lebih jelas
  - Tombol **+** dan **-** di bagian bawah
  - **Ctrl + Scroll** mouse untuk zoom cepat

### 6. Simpan Hasil
- Klik **"Simpan Hasil"**
- Pilih lokasi dan nama file
- Format sesuai pilihan di langkah 3

## Cara Build (dari Source)

```powershell
# Jalankan test terlebih dahulu
dotnet test tests\AnimeUpscaler.Core.Tests\AnimeUpscaler.Core.Tests.csproj -c Release

# Build aplikasi self-contained
dotnet publish src\AnimeUpscaler\AnimeUpscaler.csproj -c Release -r win-x64 --self-contained true -o artifacts\StellaUpscaling-win-x64
```

## Struktur Folder

```
Lumina-Upscaling/
├── src/
│   └── AnimeUpscaler/
│       ├── MainWindow.xaml        # Antarmuka pengguna
│       ├── MainWindow.xaml.cs     # Logika aplikasi
│       ├── Assets/
│       │   ├── stella_upscaling_logo.png   # Logo utama
│       │   ├── stella_upscaling_icon.png   # Icon aplikasi
│       │   └── Engine/                     # Engine & model AI
│       │       ├── realesrgan-ncnn-vulkan.exe
│       │       ├── models-v1/
│       │       ├── models-v2/
│       │       └── models-v3/
│       └── AnimeUpscaler.csproj
├── tests/                         # Unit tests
├── scripts/                       # Script build & test
├── docs/                          # Dokumentasi desain
└── artifacts/                     # Folder output build
```

## Lisensi

GUI dan integrasi desktop dalam repositori ini dibuat dari nol. Engine dan model tidak berasal dari kode GUI. Komponen pihak ketiga:

- [Real-ESRGAN](https://github.com/xinntao/Real-ESRGAN)
- [Real-ESRGAN NCNN Vulkan](https://github.com/xinntao/Real-ESRGAN-ncnn-vulkan)
- [RealSR-NCNN-Android model package](https://github.com/tumuyan/RealSR-NCNN-Android)
- [Tencent NCNN](https://github.com/Tencent/ncnn)

Salinan lisensi pihak ketiga disertakan pada `Assets/Engine/licenses`.
