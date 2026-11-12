# SonoraLab

Laboratório web de reprodução, visualização e análise de sinais sonoros, com Angular e ASP.NET Core. Permite selecionar trechos de áudio e explorar a forma de onda, o espectro e a frequência dominante.

## Executar

Requisitos — SDK .NET 10 e Node.js compatível com Angular 21, como Node.js 24.

### Backend

Na raiz do repositório.

```powershell
dotnet run --project backend/SonoraLab.Api
```

No Visual Studio, use a solução `backend/SonoraLab.slnx` e o perfil `SonoraLab.Api`. O perfil em `Properties/launchSettings.json` configura a porta **5180** e a execução no console.

### Frontend

```powershell
cd frontend
npm install
npm start
```

| Serviço | Endereço |
| --- | --- |
| Interface | http://localhost:4200 |
| API | http://127.0.0.1:5180 |
| Health check | http://127.0.0.1:5180/api/health |

O proxy em `frontend/proxy.conf.json` encaminha `/api/**` para a API. Se alterar a porta do backend, atualize esse destino.

## Uso

Carregue um **WAV PCM de 16 bits**, mono ou estéreo, de até **20 MiB e 120 segundos**. Selecione um trecho de até **5 segundos** e clique em **Analisar trecho**. A análise aceita de 4 a 524.288 amostras por canal.

O botão **Experimentar tom de 440 Hz** fornece um áudio de demonstração. Para o intervalo de 0 a 1 segundo, o pico esperado é próximo de 440 Hz.

## Verificações

```powershell
dotnet test backend/SonoraLab.slnx
npm --prefix frontend run build
```

O build do Angular e a validação HTTP com WAVs externos passaram. A conferência do upload no navegador e os demais cenários estão no [roteiro de validação](docs/etapas.md).

## Documentação

- [Arquitetura e decisões técnicas](docs/arquitetura.md)
- [Fluxos e contratos da API](docs/fluxos.md)
- [Etapas e critérios de conclusão](docs/etapas.md)
- [Interface e operação](docs/interface.md)
