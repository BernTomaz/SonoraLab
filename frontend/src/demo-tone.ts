export function demoTone(): File {
  const rate = 8000;
  const frames = 2 * rate;
  const dataBytes = frames * 2;
  const bytes = new Uint8Array(44 + dataBytes);
  const header = new DataView(bytes.buffer);
  const encoder = new TextEncoder();

  bytes.set(encoder.encode('RIFF'), 0);
  header.setUint32(4, 36 + dataBytes, true);
  bytes.set(encoder.encode('WAVEfmt '), 8);
  header.setUint32(16, 16, true);
  header.setUint16(20, 1, true);
  header.setUint16(22, 1, true);
  header.setUint32(24, rate, true);
  header.setUint32(28, rate * 2, true);
  header.setUint16(32, 2, true);
  header.setUint16(34, 16, true);
  bytes.set(encoder.encode('data'), 36);
  header.setUint32(40, dataBytes, true);

  for (let frame = 0; frame < frames; frame++) {
    const amplitude = Math.sin(2 * Math.PI * 440 * frame / rate);
    header.setInt16(44 + frame * 2, Math.round(8192 * amplitude), true);
  }

  return new File([bytes], 'tom-440-hz.wav', { type: 'audio/wav' });
}
