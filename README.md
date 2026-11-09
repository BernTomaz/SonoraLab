# SonoraLab

SonoraLab é um laboratório web de áudio voltado à reprodução, à visualização e à análise de sinais sonoros. O projeto explora a forma de onda e o conteúdo de frequência de trechos de áudio.

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

A API recebe o campo `file` em `multipart/form-data` no endpoint `POST /api/audio/inspect`. O leitor C# verifica o contêiner RIFF/WAVE e retorna metadados e a forma de onda. Limites de 20 MiB e 120 segundos.

Para experimentar sem um arquivo próprio, clique em **Carregar tom de 440 Hz**, ajuste o intervalo na forma de onda e clique em **Analisar trecho**. A FFT roda na API .NET.

## Documentação

- [Arquitetura e decisões técnicas](docs/arquitetura.md)
- [Fluxos e contratos da API](docs/fluxos.md)
- [Etapas e critérios de conclusão](docs/etapas.md)
