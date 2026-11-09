# Etapas de desenvolvimento

O MVP entrega o fluxo de importar um WAV, ouvir o áudio, selecionar um trecho e explorar suas frequências.

| Etapa | Estado | Critério de conclusão |
| --- | --- | --- |
| 1. Base | Estrutura criada | Angular consulta a API pelo proxy local do monorepo. |
| 2. Importação | Implementada no código | Um WAV PCM de 16 bits toca e exibe duração, canais e taxa de amostragem. Arquivos inválidos e incompatíveis recebem erros distintos. |
| 3. Forma de onda | Implementada no código | A API retorna mínimos e máximos por bloco. O gráfico permite selecionar um intervalo válido. |
| 4. Análise espectral | Implementada no código | Um tom sintético de 440 Hz produz um pico dentro da tolerância da resolução da FFT. Silêncio fica sem pico dominante. |
| 5. Integração | A validar no navegador | Importação, seleção e espectro funcionam de ponta a ponta, com tratamento de erros e rejeição de intervalos fora dos limites. |
| 6. Apresentação | Em andamento | README, imagens e instruções permitem executar e demonstrar o projeto. |
| 7. Evolução | Após o MVP | Adicionar espectrograma com janelas sucessivas, estimativa de nota para sons isolados, comparação entre trechos e exportação de resultados. |

## Estado da validação

Os testes do backend cobrem PCM mono e estéreo, truncamento, codificação incompatível, extremos da forma de onda, um tom de 440 Hz, silêncio e limites do intervalo. As duas rotas de áudio também foram verificadas com requisições HTTP multipart em memória. A validação do build Angular e do fluxo completo pelo navegador está pendente.

O escopo inicial exclui reconhecimento de acordes, captura de microfone, contas e histórico persistente.
