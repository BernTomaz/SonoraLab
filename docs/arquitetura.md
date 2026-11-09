# Arquitetura

O SonoraLab é um laboratório web para ouvir arquivos WAV e explorar amplitude e frequência por meio de gráficos. O processamento é numérico, em C#.

## Organização do monorepo

| Caminho | Responsabilidade |
| --- | --- |
| `frontend/` | Aplicação Angular 21 e TypeScript. Seleciona o arquivo, controla a reprodução local com HTML Audio e apresenta os resultados. |
| `backend/SonoraLab.Api/` | API HTTP em ASP.NET Core sobre .NET 10. Valida a requisição e delega a inspeção ao leitor de áudio. |
| `backend/SonoraLab.Tests/` | Testes xUnit para o leitor e, nas próximas etapas, para os cálculos de análise. |
| `backend/SonoraLab.slnx` | Solução .NET que reúne a API e os testes. |

O Git fica na raiz. Frontend e backend têm dependências e builds próprios, e alterações no contrato e no consumidor podem entrar no mesmo commit. Em desenvolvimento, o proxy do Angular encaminha `/api/**` para a API na porta `5180`.

## Decisões de implementação

| Decisão | Motivo e consequência |
| --- | --- |
| Uma API HTTP com JSON | Inspeção e análise acontecem sob demanda. Um backend atende o MVP sem SignalR, filas ou microsserviços. |
| Processamento testável fora do HTTP | O leitor C# recebe um stream. O motor espectral seguirá a mesma separação entre processamento e transporte. |
| Reprodução local e análise no servidor | O navegador toca o arquivo original. A API recebe uma cópia para validar e calcular os resultados. |
| Sem armazenamento permanente no MVP | O reenvio do arquivo a cada análise dispensa contas e histórico. A estratégia pode ser revista se a transferência prejudicar a interação. |

## Processamento e visualização

- **Atual.** O leitor C# valida RIFF/WAVE e inspeciona PCM de 16 bits, mono ou estéreo. O NAudio é a biblioteca prevista no conceito para leitura e acesso às amostras e ainda não é uma dependência da implementação.
- **Forma de onda.** A API agrupará amostras em blocos e retornará os mínimos e máximos por intervalo. Isso preserva picos e reduz o volume de dados enviado ao gráfico.
- **Espectro.** O motor extrairá as amostras do trecho, aplicará janela de Hann e calculará a FFT com Math.NET Numerics. Em estéreo, os canais serão combinados em mono para análise, com possibilidade de cancelamento quando estiverem em oposição de fase.
- **Visualização.** Canvas e SVG estão previstos para os gráficos. A resolução espectral de uma FFT sem preenchimento é a taxa de amostragem dividida pelo número de amostras, e o limite de Nyquist é metade dessa taxa.
- **Pico dominante.** É a frequência de maior magnitude, que pode corresponder a um harmônico. A estimativa posterior de nota para sons isolados deverá sinalizar incerteza em caso de ruído, silêncio ou múltiplos instrumentos.

Veja os [fluxos e contratos](fluxos.md) e as [etapas de desenvolvimento](etapas.md).
