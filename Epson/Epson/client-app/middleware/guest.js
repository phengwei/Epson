export default function ({ store, redirect }) {

  if (store.state.auth.loggedIn) {

    const userRoles = store.state.auth.user.data.roles;  
    if (userRoles.includes('Admin')) {
      return redirect('/userManagement');
    } else if (userRoles.includes('Product')) {
      return redirect('/productDashboard'); 
    } else if (userRoles.includes('Sales') || userRoles.includes('Coverplus')) {
      return redirect('/salesDashboard');
    } else if (userRoles.includes('Sales Section Head')) {
      return redirect('/request');
    } else {
      console.log('Unknown user role');
    }
  }
}
