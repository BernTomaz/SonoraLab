# Etapas de desenvolvimento

O MVP permite importar um WAV, ouvir o áudio, selecionar um trecho e explorar suas frequências.

| Etapa | Estado | Critério de conclusão |
| --- | --- | --- |
| 1. Base | Validada em execução local com o tom de teste | Angular consulta a API pelo proxy de desenvolvimento. |
| 2. Importação | Validada por HTTP com WAVs externos gerados para teste | Arquivo PCM de 16 bits é validado e disponibilizado para reprodução com os metadados corretos. |
| 3. Forma de onda | Validada com o tom embutido | Gráfico exibe extremos por bloco e permite selecionar o intervalo por arraste ou pelos campos. |
| 4. Análise espectral | Validada com o tom embutido | Um trecho de 1 s a 8.000 Hz retorna pico próximo de 440 Hz. A tela local mostrou 440,43 Hz, com FFT de 8.192 pontos. |
| 5. Integração | Parcialmente validada | Fluxo do tom embutido confirmado no navegador e upload externo validado por HTTP em memória. Falta a revisão visual do upload externo e dos estados atuais. |
| 6. Apresentação | Em andamento | Painel responsivo, favicon e documentação disponíveis. Falta registrar uma demonstração do painel atual para o README. |
| 7. Evolução | Após o MVP | Espectrograma, estimativa de nota para sons isolados, comparação entre trechos e exportação de resultados. |

## Evidências e limites

O backend compilou e suas verificações cobriram PCM mono e estéreo, truncamento, codificação incompatível, extremos da forma de onda, tom de 440 Hz, silêncio e intervalos inválidos. As rotas foram verificadas com requisições HTTP multipart em memória.

O fluxo do tom embutido foi confirmado no navegador pelo usuário. Isso valida a integração nesse cenário. WAVs independentes do tom embutido foram gerados em disco e enviados às rotas por multipart. A operação de escolher o arquivo e reproduzi-lo na interface ainda precisa de conferência manual.

O layout e o favicon foram atualizados. A revisão atual acrescenta identificação do arquivo, resumo da seleção, mensagens para intervalos inválidos, cancelamento e limite de espera. Os estados de arquivo, seleção, cancelamento, erros, limpeza e limite de espera passaram em verificações diretas do componente com Angular e RxJS. A compilação de templates e TypeScript passou. A revisão visual desses estados no navegador continua pendente. A compilação de produção do Angular também foi concluída com sucesso nesta revisão.

## Validação automatizada em 12/11/2026

| Verificação | Resultado |
| --- | --- |
| Build de produção do Angular | Aprovado. |
| Backend existente | Nove verificações aprovadas em processo único. |
| Upload e análise | Dezessete requisições multipart aprovadas com arquivos lidos do disco, usando ASP.NET Core TestServer e cultura pt-BR. |
| WAV estéreo de 44.100 Hz | Metadados e 1.024 blocos de forma de onda corretos. Dois trechos de 0,5 s retornaram 997,256 Hz e 1.499,249 Hz para sinais de 997 Hz e 1.499 Hz. Ambos ficaram dentro do espaçamento de 1,346 Hz entre bins. |
| WAV mono de 48.000 Hz com silêncio | Inspeção aceita e frequência dominante ausente. |
| Arquivos inválidos | Vazio, ausente e truncado retornaram 400. Codificação incompatível retornou 415. Arquivo acima de 20 MiB retornou 413. |
| Intervalos inválidos | Tempos negativos, invertidos, ausentes, não finitos, fora da duração, curtos demais e acima de 5 s retornaram 400. |

Os arquivos usados contêm um chunk JUNK de tamanho ímpar com preenchimento, além dos chunks PCM. Foram produzidos por um gerador independente do código de demonstração e do leitor C#.

O executor padrão dotnet test foi abortado por falha de conexão com o processo testhost após 90 segundos. As mesmas nove verificações foram chamadas diretamente e aprovadas. As requisições HTTP foram executadas em memória, sem navegador ou conexão de rede local. A reprodução e a seleção de arquivo na interface permanecem na lista de conferência manual.

## Próxima verificação manual

1. Carregar o tom de 440 Hz, reproduzir e analisar o intervalo de 0 a 1 s.
2. Escolher um WAV externo PCM de 16 bits, reproduzir e analisar um trecho válido.
3. Limpar um campo de tempo, inverter início e fim e selecionar mais que o limite permitido. Conferir as mensagens e o bloqueio do botão de análise.
4. Alterar o trecho após uma análise. O resultado anterior deve ser removido.
5. Remover o áudio ou cancelar uma operação em andamento. O painel deve voltar a um estado utilizável.
6. Conferir a navegação por teclado e a disposição dos controles em uma tela estreita.

O escopo inicial exclui reconhecimento de acordes, captura de microfone, contas e histórico persistente.
