export default function ({ $auth, $axios, $router }, inject) {
  inject('handleSsoLogin', async (samlResponse) => {
    try {
      const response = await $axios.post('/auth/AssertionConsumerService', {
        SAMLResponse: samlResponse
      });

      const token = response.data.token;
      const returnUrl = response.data.returnUrl;

      await $auth.setToken('local', token);
      $auth.setUserToken(token);

      await $auth.fetchUser();

      $router.push(returnUrl);
    } catch (error) {
      Swal.fire({
        title: 'Error!',
        text: 'SSO Login failed. Please try again.',
        icon: 'error',
        confirmButtonText: 'OK'
      });
    }
  });
}
