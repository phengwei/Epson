export default function ({ app }, inject) {
  const isClient = typeof window !== 'undefined';
  inject('isClient', isClient);
}
