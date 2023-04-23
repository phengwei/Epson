export default function ({ store, redirect }) {
    console.log("in guest.js middleware");
    console.log("store.state",store.state)
    console.log("store.state.auth.loggedIn",store.state.auth.loggedIn)
    if (store.state.auth.loggedIn) {

      console.log("store.state.auth.loggedIn == true");
      return redirect('/')
    }
  }