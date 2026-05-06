export function uniqueLabel(prefix: string): string {
  const stamp = Date.now().toString(36) + Math.random().toString(36).slice(2, 6);
  return `${prefix}-${stamp}`;
}
