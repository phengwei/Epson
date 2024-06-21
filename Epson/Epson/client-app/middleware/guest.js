export default function ({ store, redirect }) {

  if (store.state.auth.loggedIn) {

    const userRoles = store.state.auth.user.data.roles;  
    if (userRoles.includes('Admin')) {
      return redirect('/dashboard');
    } else if (userRoles.includes('Sales Section Head')) {
      return redirect('/dashboard');
    } else if (userRoles.includes('Sales Operation')) {
      return redirect('/request');
    } else if (userRoles.includes('Product') || userRoles.includes('Coverplus')) {
      return redirect('/dashboard'); 
    } else if (userRoles.includes('Sales')) {
      return redirect('/dashboard');
    } else if (userRoles.includes('Director')) {
      return redirect('/dashboard');
    } else {
      console.log('Unknown user role');
    }
  }
}
