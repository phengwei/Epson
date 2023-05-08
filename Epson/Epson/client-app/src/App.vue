<template>
    <div>
        <router-link to="/">Home</router-link>
        <router-link to="/about">About</router-link>
        <router-view></router-view>
    </div>
</template>

<script>
    import Home from './views/Home.vue'
    import About from './views/About.vue'

    export default {
        name: 'App',
        components: {
            Home,
            About
        }
    }
</script>
<style>
    #app {
        font-family: Avenir, Helvetica, Arial, sans-serif;
        -webkit-font-smoothing: antialiased;
        -moz-osx-font-smoothing: grayscale;
        text-align: center;
        color: #2c3e50;
        margin-top: 60px;
    }
</style>
