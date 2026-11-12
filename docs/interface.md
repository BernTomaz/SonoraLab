# Interface e operação

O painel segue três ações. Carregar o áudio, selecionar um trecho e analisar suas frequências. Em telas largas, a fonte e o player ficam na coluna lateral. Em telas menores, as seções passam a uma coluna.

## Carregamento e reprodução

O botão de arquivo aceita WAV e a API valida o conteúdo PCM de 16 bits, mono ou estéreo. Os limites são de 20 MiB e 120 segundos. O tom embutido de 440 Hz permite experimentar o fluxo com um áudio conhecido.

O painel identifica o arquivo por nome e tamanho, mostra o carregamento e permite cancelá-lo. Rejeitar um arquivo vazio ou acima do limite na interface preserva o áudio já carregado. A validação de conteúdo continua na API.

Depois da inspeção, o player toca o arquivo local. Duração, canais e taxa de amostragem aparecem abaixo dos controles. O botão de remoção libera a URL de reprodução e limpa seleção e resultado.

## Seleção

O usuário pode arrastar na forma de onda ou usar os campos numéricos pelo teclado. A seleção precisa conter pelo menos quatro amostras por canal, no máximo 524.288 amostras e até cinco segundos. A interface calcula o limite efetivo em segundos a partir da taxa de amostragem.

O painel mostra a duração selecionada e explica campos vazios, tempos invertidos, limites do áudio e trechos curtos ou longos demais. O botão de análise só fica disponível para uma seleção válida. Redefinir a seleção volta ao início do áudio, com até um segundo.

## Análise e resultado

A análise começa ao clicar em **Analisar trecho**. O arquivo e o intervalo são reenviados à API. Alterar a seleção cancela a requisição anterior e remove o resultado para evitar que um espectro antigo seja associado ao novo intervalo.

O painel sinaliza o processamento e permite cancelar. Cada requisição de inspeção ou análise tem limite de espera de 30 segundos. Erros de conexão, demora e validação aparecem junto aos controles relevantes. Cancelar uma requisição no navegador não garante que o servidor interrompa imediatamente todo o processamento.

O espectro apresenta a magnitude relativa ao pico e destaca a frequência dominante. Os valores numéricos permanecem disponíveis ao lado do gráfico. A ausência de pico é apresentada como resultado válido.

As explicações de janela de Hann, preenchimento com zeros e relação entre pico e nota musical ficam na seção expansível **Como interpretar o espectro**.

## Acessibilidade

Os controles têm rótulos, foco visível e áreas de acionamento de pelo menos 44 px. Campos inválidos são identificados por texto e por `aria-invalid`. Operações e resultados usam regiões de status, e os erros usam alertas. A animação de carregamento respeita a preferência de movimento reduzido.

## Limites de validação

O fluxo do tom embutido foi confirmado no navegador antes da revisão atual. O upload externo passou em 17 requisições multipart com arquivos gerados em disco, por HTTP em memória. A escolha do arquivo, sua reprodução e a revisão visual dos estados atuais precisam de conferência manual. Veja o [roteiro e as evidências](etapas.md).
