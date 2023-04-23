import { RefreshScheme } from '~auth/runtime'

export default class CustomScheme extends RefreshScheme {
  // Override `fetchUser` method of `local` scheme
  async fetchUser (endpoint) {
    this.options.endpoints.user = {
      ...this.options.endpoints.user,
      data: {
        token: this.$auth.strategy.refreshToken.get()
      }
    }

    // Token is required but not available
    if (!this.check().valid) {
      return
    }
    // User endpoint is disabled.
    if (!this.options.endpoints.user) {
      this.$auth.setUser({})
      return
    }

    console.log("this.name ",this.name);
    // Try to fetch user and then set
    return await this.$auth.requestWith(
      this.name,
      endpoint,
      this.options.endpoints.user
    ).then((response) => {
      console.log("response.data",response.data);
      const user = response.data;

      // Transform the user object
      const customUser = {
        ...user,
        fullName: user.name,
        roles: ['user']
      }

      // Set the custom user
      // The `customUser` object will be accessible through `this.$auth.user`
      // Like `this.$auth.user.fullName` or `this.$auth.user.roles`
      
      console.log("customUser",customUser);
      this.$auth.setUser(customUser)
      console.log("this.$auth.setUser",this.$auth.user);
      
      return response
    }).catch((error) => {
      this.$auth.callOnError(error, { method: 'fetchUser' })
    })
  }
}