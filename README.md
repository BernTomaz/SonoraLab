# SonoraLab

Laboratório web de áudio. A primeira entrega inspeciona WAV PCM de 16 bits, mono ou estéreo, e exibe duração, canais e taxa de amostragem. O navegador reproduz o arquivo selecionado.

## Executar

Requisitos: SDK .NET 10 e Node.js compatível com Angular 21 (por exemplo, Node.js 24).

No primeiro terminal, a partir da raiz:

```powershell
dotnet run --project backend/SonoraLab.Api --urls http://127.0.0.1:5180
```

No segundo terminal:

```powershell
cd frontend
npm install
npm start
```

Abra http://localhost:4200. O proxy de desenvolvimento encaminha `/api/**` à API.

## Testar

```powershell
dotnet test backend/SonoraLab.slnx
```

A API recebe o campo `file` em `multipart/form-data` no endpoint `POST /api/audio/inspect`. O leitor C# verifica o contêiner RIFF/WAVE e retorna metadados. Limites iniciais de 20 MiB e 120 segundos.

O próximo marco é a forma de onda com seleção de intervalo.
