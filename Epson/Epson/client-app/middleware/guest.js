export default function ({ store, redirect }) {
  console.log("in guest.js middleware");
  console.log("store.state", store.state)
  console.log("store.state.auth.loggedIn", store.state.auth.loggedIn)

  if (store.state.auth.loggedIn) {
    console.log("store.state.auth.loggedIn == true");

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
