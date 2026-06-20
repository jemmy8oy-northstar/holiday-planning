import { useEffect } from 'react';
import { useLocation } from 'react-router-dom';

const ScrollToTop = () => {
    const { pathname, state } = useLocation();

    useEffect(() => {
        if (!(state as any)?.noScroll) {
            window.scrollTo(0, 0);
        }
    }, [pathname, state]);

    return null;
};

export default ScrollToTop;
