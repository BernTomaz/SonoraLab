# SonoraLab

SonoraLab é um laboratório web de áudio voltado à reprodução, à visualização e à análise de sinais sonoros. O projeto explora a forma de onda e o conteúdo de frequência de trechos de áudio.

## Executar

Requisitos para desenvolvimento.

- SDK .NET 10.
- Node.js compatível com Angular 21, como Node.js 24.
- Para depurar pelo Visual Studio, uma versão compatível com .NET 10 e a carga de trabalho **Desenvolvimento para ASP.NET e Web**.

O backend deve escutar em `http://127.0.0.1:5180`, endereço usado pelo proxy do frontend. A interface fica em `http://localhost:4200`.

### Pelo terminal ou VS Code

Abra a pasta raiz do SonoraLab. No primeiro terminal, inicie a API.

```powershell
dotnet run --project backend/SonoraLab.Api --urls http://127.0.0.1:5180
```

No segundo terminal, inicie o Angular.

```powershell
cd frontend
npm install
npm start
```

Execute `npm install` na primeira execução e quando as dependências mudarem. Mantenha os dois terminais em execução e abra http://localhost:4200.

Para encerrar, pressione `Ctrl+C` em cada terminal.

### Backend pelo Visual Studio

1. No Visual Studio, selecione **Abrir um projeto ou uma solução** e abra `backend/SonoraLab.slnx`.
2. No Gerenciador de Soluções, clique com o botão direito em **SonoraLab.Api** e selecione **Definir como Projeto de Inicialização**.
3. Use o perfil **SonoraLab.Api**, já configurado com o endereço `http://127.0.0.1:5180`, correspondente ao proxy do Angular.

O perfil fica em `backend/SonoraLab.Api/Properties/launchSettings.json` e inicia a API no console. Para conferir a saúde da API, copie http://127.0.0.1:5180/api/health e abra manualmente no navegador.

```json
{
  "profiles": {
    "SonoraLab.Api": {
      "commandName": "Project",
      "launchBrowser": false,
      "applicationUrl": "http://127.0.0.1:5180",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

4. Selecione **SonoraLab.Api** na lista ao lado do botão de execução.
5. Pressione **F5** para iniciar com depuração ou **Ctrl+F5** para iniciar sem depuração. Com F5, os breakpoints nos controllers e no processamento de áudio serão acionados quando a interface fizer as requisições.
6. Inicie o frontend em um terminal, que pode ser o do próprio Visual Studio, o do VS Code ou o PowerShell. A partir da raiz do SonoraLab, execute os comandos abaixo.

```powershell
cd frontend
npm install
npm start
```

7. Abra http://localhost:4200. O Angular encaminha as chamadas da API para o backend em execução no Visual Studio.

Encerre o backend pelo botão **Parar Depuração** ou com **Shift+F5** quando estiver depurando. Encerre o Angular com `Ctrl+C` no terminal.

### Conferir a execução

A interface é acessada em http://localhost:4200. O endereço raiz da API não tem uma página. Acesse http://127.0.0.1:5180/api/health para verificar o backend. A API deve retornar `{"status":"ok"}`.

Na interface, clique em **Experimentar tom de 440 Hz**, mantenha o intervalo de 0 a 1 segundo e clique em **Analisar trecho**. O pico esperado é próximo de 440 Hz.

| Sintoma | O que conferir |
| --- | --- |
| A interface abre, mas não carrega o áudio | A API precisa estar em execução na porta 5180. Confira o endereço exibido pelo backend e o arquivo `frontend/proxy.conf.json`. |
| A porta 5180 está ocupada | Encerre a outra instância da API antes de iniciar a execução no Visual Studio. |
| O Visual Studio não abre a solução ou não reconhece .NET 10 | Confira a compatibilidade da instalação, o SDK .NET 10 e a carga de trabalho ASP.NET e Web. |
| O Angular não inicia | Confira a versão do Node.js e rode `npm install` dentro de `frontend`. |

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
- [Interface e operação](docs/interface.md)

## Uso do painel

O painel identifica o áudio carregado e mostra a duração do trecho selecionado. Use os campos de tempo ou arraste na forma de onda. Se o intervalo for inválido, a interface explica o motivo. Ao mudar a seleção, o espectro anterior é removido.

Carregamentos e análises podem ser cancelados e têm limite de espera de 30 segundos. Para encerrar a sessão, use o botão de remoção ao lado do nome do arquivo.

O fluxo com o tom embutido já foi confirmado no navegador. O teste com um WAV externo e a verificação dos estados atuais seguem no [roteiro de validação](docs/etapas.md).
