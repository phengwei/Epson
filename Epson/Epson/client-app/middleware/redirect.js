export default function ({ route, redirect, $auth }) {
  if (route.path === '/') {
    if ($auth.loggedIn) {
      return redirect('/dashboard');
    } else {
      return redirect('/login');
    }
  }
}
