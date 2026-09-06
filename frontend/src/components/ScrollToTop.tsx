import { useEffect } from 'react';
import { useLocation } from 'react-router-dom';

const ScrollToTop = () => {
    const { pathname, state } = useLocation();

    useEffect(() => {
        const noScroll = (state as { noScroll?: boolean } | null)?.noScroll;
        if (!noScroll) {
            window.scrollTo(0, 0);
        }
    }, [pathname, state]);

    return null;
};

export default ScrollToTop;
